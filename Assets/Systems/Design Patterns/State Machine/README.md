# Finite State Machine (FSM)


Lekki, przejrzysty i uniwersalny **Finite State Machine** napisany w C#, stworzony z myślą o integracji z silnikiem **Unity**.

- Jeden generyczny rdzeń, wielokrotnego użytku (`StateMachine<TOwner>`)  
- Obsługa **payloadów**, czyli parametrów przekazywanych do stanów  
- Możliwość dodawania i usuwania stanów  
- Metody `OnEnter`, `Tick`, `FixedTick`, `OnExit`   
- Idealny do logiki obsługi zmiennych scenariuszy, drzwi, UI, maszyn, questów.

## Struktura plików
```
📁 FSM
 ├──🧾 StateMachine.cs
 ├──📁 Extensions
    └──🧾 StateMachineExtensions.cs
 ├──📁 Utils
    └──🧾 StateMachineBehaviour.cs
 └──📁 Abstracts
    ├──🧾 State.cs
    └──🧾 IPayloadState.cs
```

## Szybki start

Utwórz klasę **przeciwnika** (*Owner*), który posiada prywatną maszynę stanów. Utwórz na `Awake` stan oraz przekaż do niego wszystkie możliwe stany. Na starcie ustawiasz domyślny - pierwszy - stan.

#### Przekazywanie wszystkich możliwych stanów daje możliwość obsługi następujących przypadków:

- Obiekty stanów tworzone są tylko raz
- Stany podczas zmiany zostawiają stare wartości (może być np stan odliczajćy na stoperze) i po włączeniu Pauzy oraz ponownego powrotu do odliczania czas nie zostanie wyzerowany
- Nie ma zagrożenia, że zostanie włączony stan, który nie powinien istnieć (np. do klasy `Budzik` nie włączysz stanu `Atakuj`)

```C#
public class Enemy : MonoBehaviour
{
    private StateMachine<Enemy> _fsm;

    private void Awake()
    {
        _fsm = new(this);
        _fsm.AddStates(new Idle(), new Chase(), new Attack());
    }

    private void Start()
    {
        _fsm.ChangeState<Idle>();
    }

    private void Update()
    {
        _fsm.Tick(Time.deltaTime);
    }
}

```

2. Napisz stany:

```c#
public class Idle : State<Enemy>
{
    public override void Tick(float dt)
    {
        if (EnemyInRange())
            Machine.ChangeState<Chase>();
    }
}

```

To naprawdę wszystko. Stany możesz przełączać przy pomocy `ChangeState`.

---

#### Przykład: proste drzwi

```c#
public class DoorClosed : State<Door>
{
    public override void OnEnter()
    {
        Owner.SetProgress(0f);
    }
}

public class DoorOpening : State<Door>
{
    float t;

    public override void OnEnter() => t = Owner.GetProgress();

    public override void Tick(float dt)
    {
        t += dt / Owner.OpenDuration;
        Owner.SetProgress(t);

        if (t >= 1f)
            Machine.ChangeState<DoorOpen>();
    }
}

public class DoorOpen : State<Door>
{
    public override void OnEnter() => Owner.SetProgress(1f);
}

public class DoorClosing : State<Door>
{
    float t;

    public override void OnEnter() => t = Owner.GetProgress();

    public override void Tick(float dt)
    {
        t -= dt / Owner.CloseDuration;
        Owner.SetProgress(t);

        if (t <= 0f)
            Machine.ChangeState<DoorClosed>();
    }
}

```

Owner:

```c#
public void Open()
{
    if (StateMachine.IsInState<DoorClosed>())
        StateMachine.ChangeState<DoorOpening>();
}
```

---

### Payload

Zmiana sceny:


```c#
public record LoadScenePayload(string SceneName);

public class LoadScene : State<GameFlow>, IPayloadState<LoadScenePayload>
{
    private LoadScenePayload _payload;

    public void SetPayload(LoadScenePayload payload) => _payload = payload;

    public override void OnEnter()
    {
        SceneManager.LoadSceneAsync(_payload.SceneName);
    }
}
```

Wywołanie:

```c#
fsm.ChangeState<GameFlow, LoadScene, LoadScenePayload>(new("Level02"));
```

---