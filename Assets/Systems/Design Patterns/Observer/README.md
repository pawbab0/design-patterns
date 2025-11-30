# Obserwator

**Wzorzec Obserwator (Observer)** to behawioralny wzorzec projektowy, który pozwala jednemu
 obiektowi (wydawcy zdarzeń) powiadamiać wiele innych obiektów (obserwatorów/słuchaczy) o
 wystąpieniu określonych zdarzeń lub zmianie swojego stanu. Dzięki temu mechanizmowi obiekty
 zainteresowane danym zdarzeniem mogą **subskrybować** powiadomienia i reagować na nie, **bez
 bezpośredniego powiązania** z obiektem wysyłającym zdarzenie. Taka architektura skutkuje **luźnym powiązaniem** komponentów – nadawca i odbiorcy nie muszą nic o sobie wiedzieć poza ustalonym interfejsem komunikacji . W Unity wzorzec Obserwatora znajduje zastosowanie np. w systemach zdarzeń gier: pozwala oddzielić logikę generującą zdarzenia (np. śmierć gracza, podniesienie przedmiotu) od kodu, który na te zdarzenia reaguje (interfejs użytkownika, system punktów, AI itp.).

 ## Implementacja

Implementacja składa się z następujących elementów:
- `IEvent` – interfejs znacznikowy dla zdarzeń.
- `IEventListener<TEvent>` – interfejs słuchacza zdarzeń określonego typu.
- `IEventBus` – interfejs szyny zdarzeń (event bus) z metodami do publikowania i zarządzania subskrypcjami.
- `EventBus` – klasa implementująca `IEventBus`, odpowiedzialna za dystrybuowanie zdarzeń do subskrybentów.
- `GlobalEventBus` – klasa statyczna udostępniająca globalny dostęp do instancji `IEventBus`.
- `EventListenerBehaviour<TEvent>` – abstrakcyjna klasa pomocnicza (MonoBehaviour) dla wygodnego tworzenia słuchaczy zdarzeń w Unity.

Poniżej znajduje się opis poszczególnych klas i interfejsów oraz wyjaśnienie, jak wspólnie realizują one wzorzec Obserwatora.

### `IEvent` (interfejs zdarzenia)

 `IEvent` to **znacznikowy interfejs** bez metod. Implementują go wszystkie klasy reprezentujące zdarzenia w systemie. Służy on głównie do oznaczenia, że dana klasa jest zdarzeniem, co umożliwia wykorzystanie mechanizmu generyków do zapewnienia **typu danych zdarzenia** w innych interfejsach/klasach (np. w `IEventListener<T>` i metodach `EventBus`).

**Jak definiować zdarzenia?** Każdy typ zdarzenia powinien być reprezentowany przez osobną klasę implementującą `IEvent`. Taka klasa może zawierać dodatkowe pola lub właściwości z informacjami o zdarzeniu (np. które obiekty są zaangażowane, ile punktów przyznano, itp.), lub może być **pusta**, jeśli sam fakt wystąpienia zdarzenia jest wystarczający do reakcji. Przykład definiowania własnego zdarzenia znajduje się w dalszej części dokumentu.

### `IEventListener<TEvent>` (interfejs słuchacza zdarzeń)

`IEventListener<TEvent>` to interfejs, który powinni implementować **wszystkie obiekty chcące nasłuchiwać określonego typu zdarzeń**. Jest to interfejs generyczny, gdzie zdarzenia, na które dany słuchacz reaguje. `TEvent` oznacza typ`TEvent` jest ograniczony do typów implementujących `IEvent`.
 
 Interfejs definiuje jedną metodę: 

```c#
    void OnEvent(TEvent evt);
```

 Metoda `OnEvent(TEvent evt)` będzie wywoływana za każdym razem, gdy poprzez system EventBus
 zostanie opublikowane zdarzenie typu 
TEvent . Implementacja tej metody w danym słuchaczu
 powinna zawierać reakcję na zajście zdarzenia (np. wykonanie pewnej logiki, aktualizacja UI,
 odtworzenie dźwięku itp.).


Dzięki związanemu z typem podejściu (`TEvent`), interfejs ten zapewnia **type safety** – słuchacz zawsze otrzyma obiekt właściwego typu zdarzenia we własnej metodzie `OnEvent`. Błędne podanie typu na etapie subskrypcji będzie sygnalizowane przez kompilator, co zapobiega pomyłkom (np. obsługa niewłaściwego rodzaju zdarzenia przez dany słuchacz).

### `IEventBus` (interfejs systemu zdarzeń)

`IEventBus` definiuje uniwersalny interfejs szyny zdarzeń (event bus), czyli mechanizmu pośredniczącego w przekazywaniu zdarzeń od nadawców do wielu potencjalnych odbiorców. Interfejs ten udostępnia trzy metody:

- `Publish<TEvent>(TEvent evt)` – Publikacja zdarzenia. Metoda przyjmuje obiekt zdarzenia typu `TEvent` (gdzie `TEvent : IEvent`) i powiadamia wszystkich subskrybentów (słuchaczy) tego typu zdarzenia. Jeśli brak słuchaczy danego typu, metoda po prostu nic nie robi. Gdy jest `null`, rzucany jest wyjątek (argument nie może być `null`).
- `Subscribe<TEvent>(IEventListener<TEvent> listener)` – Subskrybowanie (rejestrowanie) słuchacza. Metoda dodaje podany obiekt implementujący `IEventListener<TEvent>` do listy słuchaczy oczekujących na zdarzenia typu `TEvent`. Jeśli to pierwsza subskrypcja danego rodzaju zdarzeń, zostaje utworzona nowa lista. Próba dodania `null` spowoduje wyjątek. Duplikaty nie są dodawane – ten sam obiekt słuchacza zostanie dodany tylko raz.
- `Unsubscribe<TEvent>(IEventListener<TEvent> listener)` – Anulowanie subskrypcji (wyrejestrowanie) słuchacza. Usuwa podany obiekt słuchacza z listy subskrybentów zdarzeń typu `TEvent`. Jeśli lista po usunięciu jest pusta, typ zdarzenia może zostać usunięty z wewnętrznych struktur (zwolnienie zasobów). Wyrejestrowanie `null` również skutkuje wyjątkiem.


Te trzy metody definiują podstawowy protokół komunikacji w systemie: **nadawca** używa `Publish`, a odbiorcy używają `Subscribe` / `Unsubscribe` (zwykle pośrednio, np. poprzez `EventListenerBehaviour`, o czym dalej). Implementacja tych metod określona jest w klasie `EventBus`.


### `EventBus` (klasa szyny zdarzeń)

`EventBus` to konkretna implementacja interfejsu `IEventBus`. Jej zadaniem jest zarządzanie subskrybentami i dostarczanie im opublikowanych zdarzeń. `EventBus` jest oznaczony jako sealed (klasa zamknięta), co oznacza, że nie przewiduje się dziedziczenia po nim.

 Najważniejsze elementy implementacyjne `EventBus`:

- **Przechowywanie słuchaczy**: `EventBus` utrzymuje prywatną strukturę (słownik) powiązań typów zdarzeń z listami obiektów-słuchaczy. Kluczem słownika jest typ zdarzenia (`System.Type`), a wartością lista obiektów (typ ogólny `List<object>`). Każdy obiekt na liście powinien implementować odpowiedni interfejs `IEventListener<TEvent>`, co jest gwarantowane przez typ parametru w metodach 
`Subscribe` / `Unsubscribe`. Taka struktura pozwala szybko znaleźć wszystkich słuchaczy danego typu zdarzenia przy jego publikacji. `Subscribe<TEvent>(IEventListener<TEvent> listener)` : W tej metodzie kluczowym krokiem jest dodanie słuchacza do listy powiązanej z typem `TEvent`. Jeśli lista dla tego typu jeszcze nie istnieje, jest tworzona. Dodanie duplikatu jest sprawdzane – jeśli dany obiekt już subskrybuje ten event, nie zostanie dodany ponownie.
- `Unsubscribe<TEvent>(IEventListener<TEvent> listener)` : Metoda ta usuwa słuchacza z listy powiązanej z danym typem zdarzenia Jeśli po usunięciu lista staje się pusta, wpis dla tego typu jest usuwany ze słownika. Dzięki temu nie zalegają puste kolekcje dla zdarzeń, które nie mają już żadnych słuchaczy.
- `Publish<TEvent>(TEvent evt)` : Ta metoda jest wywoływana przez nadawców zdarzeń. Po upewnieniu się, że `evt` nie jest `null`, sprawdza w słowniku istnienie wpisu dla typu danego zdarzenia. Jeśli nie ma żadnych słuchaczy tego typu, metoda kończy działanie (brak odbiorców = brak akcji). Jeśli lista istnieje, następuje iteracja po kopii listy słuchaczy i wywołanie u każdego z nich metody `OnEvent(evt)`.

Użycie kopii (`ToArray()`) listy przed iteracją ma na celu zabezpieczenie przed modyfikacją kolekcji w trakcie iterowania (np. gdyby w wyniku wywołania `OnEvent` jakiś słuchacz wyrejestrował się lub zarejestrował nowe zdarzenie). W ten sposób publikowanie zdarzenia jest bezpieczne i nie spowoduje wyjątku nawet, jeśli listy słuchaczy zmienią się w czasie obsługi zdarzenia. Każdy obiekt z listy zostaje rzutowany na `IEventListener<TEvent>` i powiadomiony o zdarzeniu poprzez wywołanie `OnEvent`. W praktyce, rzutowanie zawsze powinno się powieść, ponieważ do listy dodajemy tylko obiekty implementujące właściwy interfejs (jest to  kontrolowane przez sygnaturę metody `Subscribe`).

Warto zauważyć, że `EventBus` sam w sobie **nie zna konkretnych klas zdarzeń ani słuchaczy** - wszystko odbywa się generycznie i w oparciu o interfejsy. Dzięki temu jest on uniwersalny i może obsłużyć dowolną liczbę różnych typów zdarzeń.


###  `GlobalEventBus` (globalna instancja)

`GlobalEventBus` to statyczna klasa pomocnicza udostępniająca globalny dostęp do instancji `IEventBus`. Pozwala to na wygodne użycie systemu zdarzeń bez konieczności przekazywania referencji do `EventBus` między obiektami lub utrzymywania go w kontekście sceny. Definicja `GlobalEventBus` jest prosta:

```c#
public static class GlobalEventBus 
{
    private static IEventBus _instance = new EventBus();

    public static IEventBus Instance
    {
        get => _instance;
        set => _instance = value ?? throw new ArgumentNullException(nameof(value));
    }
}
```

Domyślnie `GlobalEventBus.Instance` jest ustawiony na nową instancję klasy publiczną właściwość `EventBus`. Poprzez `Instance` można też (jeśli zajdzie taka potrzeba) przypisać własny obiekt implementujący `IEventBus`. Zapewnia to elastyczność – np. w testach jednostkowych można podmienić globalny `EventBus` na atrapę (mock) lub gdy projekt wymaga istnienia kilku niezależnych busów zdarzeń, można sterować, której instancji używają różne części aplikacji. W typowym użyciu jednak korzysta się po prostu z `GlobalEventBus.Instance` dostarczonego przez tę klasę.


**Uwaga**: Korzystanie z globalnej instancji oznacza, że wszystkie obiekty w aplikacji będą domyślnie publikować i subskrybować zdarzenia na tej jednej wspólnej szynie. Jest to wygodne i zazwyczajpożądane (centralny system komunikacji). Należy jednak mieć na uwadze, że bardzo rozbudowany system zdarzeń z ogromną liczbą różnych zdarzeń może w skrajnych przypadkach wymagać podziału na mniejsze konteksty (np. osobne `EventBusy` dla modułów niezależnych od siebie).

### `EventListenerBehaviour<TEvent>` (klasa bazowa słuchacza `MonoBehaviour`)

`EventListenerBehaviour<TEvent>` to **abstrakcyjna klasa MonoBehaviour**, która **upraszcza tworzenie słuchaczy zdarzeń w Unity**. Dziedzicząc po niej, można szybko utworzyć komponent, który automatycznie subskrybuje globalny EventBus i odbiera zdarzenia danego typu. Klasa ta implementuje interfejs `IEventListener<TEvent>` i wymusza implementację metody `OnEvent(TEvent evt)` w klasie pochodnej (jest to metoda abstrakcyjna).

 Najważniejsze cechy `EventListenerBehaviour<TEvent>`: **Automatyczna subskrypcja**: Obiekt będący komponentem Unity (MonoBehaviour) automatycznie rejestruje się w globalnym EventBusie w momencie aktywacji (włączenia) obiektu. Dzieje się to w metodzie `OnEnable()`, która wywołuje `EventBus.Subscribe(this)`. Dzięki temu nie trzeba ręcznie subskrybować zdarzeń w każdym takim komponencie – wystarczy, że jest on aktywny. - Automatyczne wyrejestrowanie: Gdy obiekt zostaje zdezaktywowany lub zniszczony (wywoływana jest metoda `OnDisable()`), następuje automatyczne wycofanie subskrypcji z EventBusa (`EventBus.Unsubscribe(this)`). To zabezpiecza przed sytuacją, gdzie zniszczony lub wyłączony obiekt nadal figuruje na liście słuchaczy (co mogłoby powodować błędy lub odwołania do zniszczonych obiektów). Mechanizm ten sprawia, że zarządzanie cyklem życia subskrypcji jest zgodne z cyklem życia obiektu w Unity. - Właściwość `EventBus`: Klasa udostępnia chronioną właściwość `EventBus`, która domyślnie zwraca `GlobalEventBus.Instance`. Jest ona oznaczona jako `virtual` , co oznacza, że ewentualnie klasa dziedzicząca może nadpisać ją, by skierować subskrypcje do innej instancji `IEventBus` (np. lokalnego EventBusa zamiast globalnego, jeśli ktoś zdecyduje się na taką architekturę). W typowych zastosowaniach nie ma potrzeby tego zmieniać – korzystamy z domyślnego globalnego busa. - Wymagana implementacja `OnEvent`: Ponieważ `EventListenerBehaviour` jest klasą abstrakcyjną, każda klasa pochodna musi zaimplementować metodę public override void OnEvent(TEvent evt) . W tej metodzie umieszczamy logikę, która ma się wykonać w reakcji na nadejście danego zdarzenia.

**Przykład użycia** `EventListenerBehaviour`: Załóżmy, że mamy zdarzenie `PlayerDiedEvent`. Możemy utworzyć komponent: 

```c#
public class GameOverListener : EventListenerBehaviour<PlayerDiedEvent>
{
    public override void OnEvent(PlayerDiedEvent evt)
    {
        // Reakcja na śmierć gracza, np. wyświetlenie ekranu "Game Over"
        UIManager.ShowGameOver();
    }
}
```

Taki komponent, dodany do odpowiedniego obiektu (np. obiektu UI odpowiedzialnego za ekran końca gry), będzie automatycznie  subskrybował zdarzenia `PlayerDiedEvent` podczas aktywacji obiektu. Gdy zdarzenie zostanie opublikowane w systemie, metoda 
`OnEvent` wykona kod wyświetlający ekran "Game Over".


##  Przykłady użycia systemu zdarzeń w Unity

Aby lepiej zilustrować praktyczne wykorzystanie powyższego systemu, poniżej przedstawiono dwa proste scenariusze zdarzeń w grze Unity: **śmierć gracza** oraz **podniesienie przedmiotu**. Każdy przykład obejmuje definicję klasy zdarzenia, implementację przykładowego słuchacza oraz sposób publikacji zdarzenia.

### Przykład 1: Zdarzenie śmierci gracza (`OnPlayerDied`)

Załóżmy, że chcemy powiadamiać różne części gry o tym, że gracz zginął – na przykład, by wyświetlić ekran końcowy oraz zaktualizować interfejs liczby żyć. Możemy to osiągnąć definiując zdarzenie `PlayerDiedEvent` i odpowiednich słuchaczy.

**Definicja zdarzenia `PlayerDiedEvent`:**

```c#
 public class PlayerDiedEvent : IEvent
 {
    public GameObject gracz; // który obiekt gracza zginął
    public int pozostaleZycia; // ile żyć zostało (np. do wyświetlenia)

    public PlayerDiedEvent(GameObject gracz, int pozostaleZycia)
    {
        this.gracz = gracz;
        this.pozostaleZycia = pozostaleZycia;
    }
}
```

Powyższa klasa implementuje `IEvent`, dzięki czemu może być używana w naszym systemie. Zawiera informacje o graczu (referencja do obiektu gracza) oraz np. liczbę pozostałych żyć, co pozwoli słuchaczom zareagować odpowiednio (np. wyświetlić to na ekranie).

**Implementacja słuchacza – wyświetlenie ekranu Game Over:**

```c#
 public class GameOverListener : EventListenerBehaviour<PlayerDiedEvent>
 {
    public override void OnEvent(PlayerDiedEvent evt)
    {
        // Przykładowa reakcja: wyświetlenie ekranu "Game Over"
        UIManager.ShowGameOver();
        
        // Możemy również użyć danych z evt, np. zablokować sterowanie  graczem:
        evt.gracz.SetActive(false);
    }
}
```

Komponent `GameOverListener` (dziedziczący po `EventListenerBehaviour<PlayerDiedEvent>`) automatycznie subskrybuje zdarzenia.`PlayerDiedEvent`. W momencie otrzymania zdarzenia wywoła metodę `UIManager.ShowGameOver()` (zakładamy istnienie takiej metody statycznej do obsługi UI) oraz np. dezaktywuje obiekt gracza, który zginął. Tę klasę można dodać jako komponent do obiektu interfejsu użytkownika odpowiedzialnego za ekran końcowy.

**Publikacja zdarzenia `PlayerDiedEvent`:**

W momencie gdy gracz ginie (np. w skrypcie gracza lub kontrolera rozgrywki), wystarczy wywołać publikację zdarzenia na globalnym EventBusie, np.:


```c#
 // Fragment kodu w skrypcie odpowiedzialnym za życie gracza:
 if (currentHealth <= 0)
 {
    // Utworzenie instancji zdarzenia z odpowiednimi danymi
    var deathEvent = new PlayerDiedEvent(gameObject, remainingLives);
    
    // Publikacja zdarzenia - powiadomienie wszystkich słuchaczy
    GlobalEventBus.Instance.Publish(deathEvent);
 }
 ```


 ### Przykład 2: Zdarzenie podniesienia przedmiotu (`OnItemPicked`)

 W drugim scenariuszu, rozważmy mechanikę podnoszenia przedmiotów. Gdy gracz podnosi przedmiot, chcemy powiadomić różne systemy: np. system ekwipunku (aby dodać przedmiot do listy), UI (aby wyświetlić komunikat lub ikonę przedmiotu) oraz system dźwięku (aby zagrać odpowiedni efekt audio). Ponownie, wykorzystamy wzorzec Obserwatora, definiując zdarzenie `ItemPickedEvent` i kilka niezależnych słuchaczy.

**Definicja zdarzenia `ItemPickedEvent`:**


```c#
public class ItemPickedEvent : IEvent
{
    public string itemName; // nazwa lub ID podniesionego przedmiotu
    public GameObject itemObject; // referencja do obiektu przedmiotu (jeśli potrzebna)
 
    public ItemPickedEvent(string itemName, GameObject itemObject)
    {
        this.itemName = itemName;
        this.itemObject = itemObject;
    }
}
```

To zdarzenie przekazuje informację o tym, jaki przedmiot został podniesiony (np. nazwę identyfikującą go) oraz opcjonalnie referencję do obiektu w świecie (co może być wykorzystane np. do dezaktywacji go lub odtworzenia cząsteczek). Można oczywiście rozszerzyć to o dodatkowe dane, jak np. ilość przedmiotów, które zebrano.


**Implementacja słuchacza – system ekwipunku:**

```c#
public class InventoryListener : EventListenerBehaviour<ItemPickedEvent>
{
    public Inventory inventory;  // referencja do logiki ekwipunku (np. skrypt przechowujący przedmioty)
    
    public override void OnEvent(ItemPickedEvent evt)
    {
        // Dodaj przedmiot do ekwipunku gracza
        inventory.AddItem(evt.itemName);
        
        // (Opcjonalnie) Dezaktywuj obiekt przedmiotu w świecie, bo został podniesiony
        if(evt.itemObject != null)
            evt.itemObject.SetActive(false);
    }
}
```

Ten komponent `InventoryListener`, przypięty np. do obiektu zarządzającego ekwipunkiem, nasłuchuje zdarzeń `ItemPickedEvent`. Po odebraniu zdarzenia dodaje przedmiot o danej nazwie do wewnętrznej listy/danych ekwipunku (metoda `inventory.AddItem(...)`) oraz dezaktywuje obiekt przedmiotu w scenie (co zakładamy jako sposób na "usuniecie" go z świata po podniesieniu). Dzięki temu logika zbierania przedmiotów jest odseparowana od mechaniki samego gracza.


**Implementacja słuchacza – interfejs użytkownika (powiadomienie):**

```c#
public class ItemPickupUIListener : EventListenerBehaviour<ItemPickedEvent>
{
    public override void OnEvent(ItemPickedEvent evt)
    {
        // Wyświetl komunikat lub ikonę podniesionego przedmiotu
        Debug.Log($"Podniesiono przedmiot: {evt.itemName}");
        // (Można tu np. zaktualizować UI ekwipunku, pokazać ikonkę itp.)
    }
}
```

Ten prosty słuchacz wypisuje w konsoli informację o podniesieniu przedmiotu (w praktyce zamiast `Debug.Log` można wywołać metody UI, np. wyświetlenie notyfikacji w grze). Ważne jest, że może on działać niezależnie od systemu ekwipunku – pełni inną rolę, ale reaguje na to samo zdarzenie.

**Implementacja słuchacza – efekt dźwiękowy:**

```c#
public class ItemPickupSoundListener : EventListenerBehaviour<ItemPickedEvent>
{
    public AudioSource audioSource;
    public AudioClip pickupClip;
    
    public override void OnEvent(ItemPickedEvent evt)
    {
        // Odtwórz dźwięk podniesienia przedmiotu
        audioSource.PlayOneShot(pickupClip);
    }
}
```

Ten trzeci słuchacz (podłączony np. do obiektu audio managera) nasłuchuje `ItemPickedEvent` i w reakcji odtwarza przypisany dźwięk. Dzięki temu za każdym razem, gdy dowolny przedmiot jest podniesiony, słyszymy odpowiedni efekt audio.

**Publikacja zdarzenia `ItemPickedEvent`:**
Zakładamy, że w logice gracza lub przedmiotu jest moment, w którym następuje zebranie przedmiotu. Np. gracz wchodzi w kolizję z obiektem przedmiotu. Wówczas w odpowiednim skrypcie możemy napisać:

```C#
// Fragment w skrypcie obsługi kolizji gracza z przedmiotem:
void OnTriggerEnter(Collider other)
{
    if(other.CompareTag("Item"))
    {
        // Pobierz nazwę przedmiotu z komponentu Item (przykładowo)
        string itemName = other.GetComponent<Item>().itemName;
        
        // Opublikuj zdarzenie podniesienia przedmiotu
        var itemEvent = new ItemPickedEvent(itemName, other.gameObject);
        GlobalEventBus.Instance.Publish(itemEvent);
    }
}
```

W tym kodzie, gdy wykryjemy kolizję z obiektem oznaczonym tagiem "Item", tworzymy zdarzenie `ItemPickedEvent` zawierające nazwę tego przedmiotu oraz referencję do obiektu (który możemy następnie zdezaktywować przez słuchacza, jak pokazano wyżej). Wywołujemy `Publish`, co powoduje natychmiastowe powiadomienie wszystkich zarejestrowanych słuchaczy ItemPickedEvent: systemu ekwipunku (`InventoryListener`), UI (`ItemPickupUIListener`), systemu dźwięku (`ItemPickupSoundListener`), a także innych, jeśli zostałyby dodane. Każdy z nich wykonuje swoją logikę: dodanie itemu, pokazanie komunikatu, zagranie dźwięku, itp. – wszystko to równolegle, bez wzajemnej wiedzy o swoim istnieniu, poza faktem reagowania na dane zdarzenie.

Dzięki temu podejściu łatwo można dodawać lub usuwać reakcje na zdarzenie podniesienia przedmiotu nie modyfikując kodu gracza ani przedmiotu – wystarczy utworzyć/usunąć odpowiedniego słuchacza.


## Dodawanie własnych zdarzeń i słuchaczy

System Observer/EventBus jest łatwy do rozszerzania. Aby dodać nowy typ zdarzeń do swojego projektu i reagujących na nie słuchaczy, postępuj według poniższych kroków:

1. **Utwórz klasę zdarzenia:** Zdefiniuj nową klasę implementującą `IEvent`. Nadaj jej nazwę opisującą zdarzenie, np. `EnemySpawnedEvent`, `ScoreChangedEvent` itp. Dodaj do klasy pola lub właściwości, które chcesz przekazywać wraz ze zdarzeniem (np. ilość punktów, pozycję przeciwnika, cokolwiek istotnego dla kontekstu wydarzenia). Jeśli zdarzenie nie wymaga przekazywania danych, klasa może pozostać pusta w środku (ważne, by implementowała `IEvent`). Przykład:

```c#
public class EnemySpawnedEvent : IEvent 
{
    public Vector3 spawnPosition;
    public EnemySpawnedEvent(Vector3 position)
    {
        spawnPosition = position;
    }
}
```

Taka klasa reprezentuje zdarzenie pojawienia się nowego wroga w grze, przenosząc informację o współrzędnych spawnu.

**Uwaga**: Tworzyć można również `struktury` typu `readonly`.

2.** Utwórz słuchacza (lub wielu słuchaczy):** Zastanów się, które obiekty/systemy powinny reagować na to zdarzenie. Dla każdej takiej reakcji stwórz albo:

    - Komponent Unity oparty o `EventListenerBehaviour<TEvent>`: Najwygodniejsza opcja, jeśli reakcja dotyczy obiektu w scenie lub funkcjonalności, która może być zawarta w komponencie. Dziedzicz po `EventListenerBehaviour<EnemySpawnedEvent>` i zaimplementuj `OnEvent(EnemySpawnedEvent evt)`. Wewnątrz metody napisz, co ma się stać, gdy nowy wróg się pojawi (np. zaktualizuj licznik wrogów, odtwórz animację alarmu itp.). Następnie dodaj ten komponent do odpowiedniego GameObjectu w scenie (np. menedżera fal przeciwników, UI itp.).
    - Lub klasę nie będącą MonoBehaviourem, implementującą `IEventListener<EnemySpawnedEvent>`: Ta opcja może być użyta dla obiektów tworzonych na bieżąco lub systemów działających poza Unity (np. czysto logiczne klasy). W takiej klasie musisz samodzielnie zadbać o rejestrację: np. wywołać `GlobalEventBus.Instance.Subscribe<EnemySpawnedEvent>(this)` gdy obiekt powinien zacząć nasłuchiwać (i `Unsubscribe` gdy już nie powinien). Zyskujesz jednak pełną swobodę co do struktury klasy. Przykład:

```c#
public class EnemyCounter : IEventListener<EnemySpawnedEvent>
{
    public int totalEnemies = 0;
    public void OnEvent(EnemySpawnedEvent evt)
    {
        totalEnemies++;
        Debug.Log("Spawned enemies count = " + totalEnemies);
    }
}

// ... gdzieś w kodzie inicjalizującym:
var counter = new EnemyCounter();
GlobalEventBus.Instance.Subscribe<EnemySpawnedEvent>(counter);
```

Powyższy kod pokazuje obiekt zliczający pojawiających się wrogów. Po utworzeniu instancji rejestrujemy go na globalnym EventBusie. Pamiętaj, by w odpowiednim momencie wyrejestrować takie obiekty (np. przy zakończeniu poziomu, sceny, lub gdy obiekt nie jest już potrzebny) – inaczej EventBus może przechowywać referencję i zapobiec zwolnieniu obiektu przez GC.

3. **Publikuj nowe zdarzenie w odpowiednim momencie gry:** Zidentyfikuj miejsce w kodzie, gdzie dane zdarzenie powinno być wywoływane. Następnie utwórz instancję swojego zdarzenia i opublikuj je przez `GlobalEventBus`. W naszym przykładzie `EnemySpawnedEvent` byłby publikowany tuż po stworzeniu nowego wroga:

```c#
// fragment kodu spawnera wrogów:
Enemy newEnemy = SpawnEnemy();  // tworzenie nowego przeciwnika

// Publikacja zdarzenia o pojawieniu się przeciwnika
GlobalEventBus.Instance.Publish(new EnemySpawnedEvent(newEnemy.transform.position));
```

Spowoduje to natychmiastowe powiadomienie wszystkich zarejestrowanych słuchaczy `EnemySpawnedEvent` (np. nasz `EnemyCounter` czy inne systemy).

---

Podsumowując, dodanie nowego zdarzenia sprowadza się do napisania klasy zdarzenia i tylu słuchaczy, ile potrzeba dla reakcji w grze. Reszta mechaniki (rejestrowanie, powiadamianie) jest już gotowa w dostarczonych klasach i **nie wymaga modyfikacji**.