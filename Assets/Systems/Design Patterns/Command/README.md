# Command

Uniwersalny, asynchroniczny system Command dla Unity 6 z obsługą Undo, limitem historii, komendami złożonymi oraz wsparciem `Awaitable`.
Zaprojektowany tak, aby można było podpiąć dowolny system — gameplay, UI, ekwipunek, edytory, narzędzia developerskie i wiele innych.

## Opis systemu

System implementuje wzorzec Command z dostosowaniem do Unity 6:

- operacje są asynchroniczne (`Awaitable`),
- możliwe jest cofanie działań (Undo),
- istnieje limit historii cofania,
- komendy mogą być złożone (Composite Pattern),
- komendy mogą, ale nie muszą przyjmować kontekstu,

Mechanizm jest niezależny od konkretnych systemów gry i może być wykorzystany w dowolnym projekcie Unity, zarówno runtime jak i edytorowym.

## Główne założenia

- Asynchroniczność — wszystkie komendy implementują `ExecuteAsync` oraz `UndoAsync`.
- Historia Undo — limitowana, FIFO, automatycznie przycinana po przekroczeniu limitu najstarsze komendy.
- Bezpieczeństwo wykonania — `CommandExecutor` zapewnia, że tylko jedna komenda może być wykonywana naraz.
- Modularność — kontekst komendy (`TContext`) pozwala dopasować Command Pattern do różnych systemów.
- Komendy złożone — możliwość skupienia wielu komend w jeden batch.
- Prosta integracja — jeden executor na system, brak skomplikowanej konfiguracji.

## Przykład użycia

```C#
var executor = new CommandExecutor<Transform>(maxUndoSteps: 20);

var move = new MoveTransformCommand(direction: Vector3.forward);

// Wykonanie komendy:
await executor.ExecuteAsync(move, player.transform);

// Cofnięcie:
await executor.UndoAsync(player.transform);
```

## Komendy złożone (CompositeCommand)

- wykonuje wiele komend sekwencyjnie,
- może działać w trybie „transakcyjnym” (rollback w razie błędu),
- obsługuje Undo całej grupy naraz.


```C#
var composite = new CompositeCommand<GameplayContext>(
    "MoveForwardSequence",
    rollbackOnFailure: true,
    new MoveTransformCommand(Vector3.forward),
    new MoveTransformCommand(Vector3.forward)
);

await executor.ExecuteAsync(composite, context);
```

## Integracja z Unity

```C#
public class PlayerInputExample : MonoBehaviour
{
    private CommandExecutor<Transform> _executor;

    private void Awake()
    {
        _executor = new CommandExecutor<GameplayContext>(20);
    }

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            await _executor.ExecuteAsync(new MoveTransformCommand(Vector3.forward), transform);

        if (Input.GetKeyDown(KeyCode.Z))
            await _executor.UndoAsync(transform);
    }
}
```


## Najlepsze praktyki

- Utrzymuj kontekst prosty i jasno określony.
- Dla powtarzalnych operacji (np. animowane ruchy) trzymaj wszystkie dane do Undo w samej komendzie.
- Dziel komendy na proste, małe elementy — łatwiej wtedy je komponować.
- Kompozyty używaj do batchy i transakcji logicznych.
- Twórz jeden executor na system (np. gameplay, UI, edytor map).