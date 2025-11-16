using UnityEngine;

[CreateAssetMenu(fileName = "MovementConfig", menuName = "GameConfigs/Movement Config")]
public class MovementConfig : ScriptableObject
{
    [Tooltip("Включает отладочный режим")]
    public bool DebugMode = false;

    [Header("COLLISIONS")]
    [Tooltip("Какие слои считать землёй")]
    public LayerMask GroundLayer;

    [Tooltip("Какие слои считать стенами")]
    public LayerMask WallLayer;

    [Tooltip("По каким слоям героиня будет скользить")]
    public LayerMask SlidingLayer;

    [Tooltip("Расстояние проверки коллизий")]
    public float CollisionOffset = 0.01f;

    [Tooltip("Расстояние проверки касания стен")]
    public float WallCollisionDistance = 0.01f;

    [Tooltip("Сила притяжения")]
    public float Gravity = 25f;

    [Tooltip("Шаг восстановления заноса при движении")]
    public float DragResetStep = 0.8f;

    [Header("BASIC MOVEMENT")]
    [Header("Running")]
    [Tooltip("Скорость движения")]
    public float MovementSpeed = 6f;

    [Tooltip("Ускорение на земле (чем больше, тем быстрее ускоряемся)")]
    public float GroundAcceleration = 75f;

    [Tooltip("Замедление на земле (чем больше, тем быстрее замедляемся)")]
    public float GroundDeceleration = 60f;

    [Tooltip("Дистанция до земли, при которой героиня будет продолжать бежать (не начинать падать)")]
    public float KeepRunningDistance = 3f;

    [Header("Jump")]
    [Tooltip("Сила прыжка")]
    public float JumpForce = 10.5f;

    [Tooltip("Ускорение в прыжке (чем больше, тем быстрее ускоряемся)")]
    public float InJumpAcceleration = 45f;

    [Tooltip("Замедление в прыжке (чем больше, тем быстрее замедляемся)")]
    public float InJumpDeceleration = 45f;

    [Header("Wall Jump")]
    [Tooltip("Сила прыжка от стены")]
    public float WallJumpForce = 15f;

    [Range(0, 1)]
    [Tooltip("Сила заноса при прыжке от стены")]
    public float WallJumpDrag = 0.2f;

    [Header("Falling")]
    [Tooltip("Максимальная скорость падения")]
    public float MaxFallSpeed = 10f;

    [Tooltip("Ускорение в падении (чем больше, тем быстрее ускоряемся)")]
    public float InFallAcceleration = 45f;

    [Tooltip("Замедление в падении (чем больше, тем быстрее замедляемся)")]
    public float InFallDeceleration = 45f;

    [Header("Angle Grab and Climb")]
    [Tooltip("Время, в течение которого героиня будет прилипать к стене")]
    public float GrabAngleDuration = 0.3f;

    [Tooltip("Время перезарядки способности 'схватиться за стену'")]
    public float GrabCooldown = 0.5f;

    [Tooltip("Сила вскарабкивания вверх на угол")]
    public float WallClimbUpForce = 10f;

    [Tooltip("Время вскарабкивания вверх на угол")]
    public float WallClimbUpTime = 0.5f;

    [Tooltip("Сила бокового движения при вскарабкивании на угол")]
    public float WallClimbSideForce = 5f;

    [Header("Sliding Slope")]
    [Tooltip("Максимальная скорость скольжения по склону")]
    public float MaxSlopeSlidingSpeed = 7f;

    [Range(0, 1)]
    [Tooltip("Занос в воздухе после прыжка в состоянии скольжения по склону")]
    public float SlopeSlidingJumpDrag = 0.2f;

    [Range(0, 1)]
    [Tooltip("Занос в воздухе после прыжка в состоянии скольжения по склону")]
    public float SlopeSlidingFallDrag = 0.1f;

    [Header("Sliding Wall")]
    [Tooltip("Скорость скольжения по стене")]
    public float SlidingWallSpeed = 4f;

    [Tooltip("Сила замедления при скольжении по стене")]
    public float SlidingWallDeceleration = 0.5f;

    [Header("Air Abilities")]
    [Tooltip("Сила ветряного отскока")]
    public float AirBounceForce = 9f;

    [Tooltip("Время зарядки ветряного отскока")]
    public float AirBounceChargeTime = 0.5f;

    [Tooltip("Дистанция до земли ветряного отскока")]
    public float AirBounceGroundDistance = 5f;

    [Tooltip("Скорость ветряной волны")]
    public float AirWaveSpeed = 10f;

    [Tooltip("Время жизни ветряной волны")]
    public float AirWaveLifeTime = 0.5f;

    [Tooltip("Время перезарядки ветряной волны")]
    public float AirWaveCooldown = 1f;

    [Tooltip("Сила ветряной волны")]
    public float AirWaveImpulseForce = 20f;

    [Range(0, 1)]
    [Tooltip("Занос в воздухе после отталкивания ветряной волной")]
    public float AirWaveDrag = 0.2f;

    [Tooltip("Минимальная сила ветряной волны")]
    public float AirWaveMinCharge = 0.5f;

    [Tooltip("Максимальная сила ветряной волны")]
    public float AirWaveMaxCharge = 1f;

    [Tooltip("Длительность левитации в состоянии 'воздушной волны'")]
    public float AirWaveLevitationDuration = 2f;

    [Header("Sliding Ability")]
    [Tooltip("Максимальная скорость скольжения по земле при использовании способности 'скольжение'")]
    public float MaxSlidingSpeed = 10f;

    [Tooltip("Сила замедления при скольжении по земле")]
    public float SlidingDeceleration = 0.5f;

    [Tooltip("Минимальная скорость скольжения по земле")]
    public float MinSlidingSpeed = 4f;

    [Tooltip("Ускорение при скольжении по земле (чем больше, тем быстрее ускоряемся)")]
    public float SlidingAcceleration = 1f;
}
