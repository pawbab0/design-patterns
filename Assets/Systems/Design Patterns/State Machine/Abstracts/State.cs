namespace PawBab.DesignPatterns.FSM
{
    /// <summary>
    /// Bazowa klasa stanu dla uniwersalnej maszyny stanów.
    /// <para>
    /// Stan reprezentuje konkretny „tryb pracy” obiektu – np. <c>Idle</c>, <c>Moving</c>,
    /// <c>Attacking</c>, <c>OpeningDoor</c> itp. Każdy stan zna swojego właściciela
    /// (<typeparamref name="TOwner"/>) oraz maszynę stanów, do której należy.
    /// </para>
    /// <para>
    /// Typ <typeparamref name="TOwner"/> zwykle będzie klasą logiki (np. komponentem
    /// <c>MonoBehaviour</c>), ale nie ma takiego ograniczenia – może to być dowolny typ.
    /// </para>
    /// </summary>
    /// <typeparam name="TOwner">
    /// Typ obiektu, którego zachowaniem zarządza maszyna stanów. Najczęściej jest to
    /// komponent Unity (np. <c>PlayerController</c>, <c>EnemyAI</c>, <c>Door</c>).
    /// </typeparam>
    public abstract class State<TOwner>
    {
        /// <summary>
        /// Obiekt, którego zachowanie jest zarządzane przez dany stan.
        /// <para>
        /// Używaj tej właściwości wewnątrz stanów do wykonywania operacji na właścicielu,
        /// np. poruszania go, zmiany animacji, wywoływania metod itp.
        /// </para>
        /// </summary>
        protected TOwner Owner { get; private set; }

        /// <summary>
        /// Maszyna stanów, do której należy ten stan.
        /// <para>
        /// Pozwala m.in. na ręczne przełączanie stanów z poziomu stanu,
        /// np. <c>Machine.ChangeState&lt;SomeOtherState&gt;()</c>.
        /// </para>
        /// </summary>
        protected StateMachine<TOwner> Machine { get; private set; }

        /// <summary>
        /// Inicjalizuje stan referencjami do właściciela i maszyny stanów.
        /// <para>
        /// Metoda jest wywoływana automatycznie przez <see cref="StateMachine{TOwner}.AddState"/>
        /// – nie należy jej wywoływać ręcznie w kodzie użytkownika.
        /// </para>
        /// </summary>
        /// <param name="owner">Obiekt, którego zachowanie reprezentuje ten stan.</param>
        /// <param name="machine">Maszyna stanów zarządzająca cyklem życia tego stanu.</param>
        internal void Initialize(TOwner owner, StateMachine<TOwner> machine)
        {
            Owner = owner;
            Machine = machine;
        }

        /// <summary>
        /// Logika wykonywana przy wejściu w stan.
        /// <para>
        /// Wywoływana raz po przełączeniu na ten stan, po wywołaniu <c>OnExit()</c>
        /// na poprzednim stanie (jeśli taki istniał).
        /// </para>
        /// </summary>
        public virtual void OnEnter()
        {
        }

        /// <summary>
        /// Logika wykonywana przy wyjściu ze stanu.
        /// <para>
        /// Wywoływana raz bezpośrednio przed aktywacją nowego stanu, gdy maszyna
        /// stanów dokonuje przełączenia.
        /// </para>
        /// </summary>
        public virtual void OnExit()
        {
        }

        /// <summary>
        /// Aktualizacja logiki stanu wywoływana w każdej klatce (odpowiednik <c>Update</c>).
        /// <para>
        /// Warto korzystać z przekazanego <paramref name="deltaTime"/>, zamiast bezpośrednio
        /// z <c>Time.deltaTime</c>, aby zachować testowalność i niezależność od Unity.
        /// </para>
        /// </summary>
        /// <param name="deltaTime">Czas, jaki upłynął od poprzedniej klatki.</param>
        public virtual void Tick(float deltaTime)
        {
        }

        /// <summary>
        /// Aktualizacja logiki stanu wywoływana w każdym kroku fizyki
        /// (odpowiednik <c>FixedUpdate</c>).
        /// </summary>
        /// <param name="fixedDeltaTime">Czas kroku fizyki.</param>
        public virtual void FixedTick(float fixedDeltaTime)
        {
        }
    }
}