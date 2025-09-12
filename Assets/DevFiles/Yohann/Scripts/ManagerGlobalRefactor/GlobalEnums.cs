using UnityEngine;
// GLOBAL ENUMS
// Defines a collection of enums used throughout the project to represent
// various types, roles, actions, and directions relevant to the game's logic. These enums
// include input hands, thumbstick directions, item types, form types, fingerprint tools and powders,
// player roles, game actions, and evidence marker types. Centralizing these enums helps maintain
// consistency and type safety across the codebase, making it easier to manage and reference
// these categories in gameplay scripts and systems.

public enum InputHand
{
    Left,
    Right
}

public enum ThumbstickDirection
{
    None,
    Up,
    Down,
    Left,
    Right
}
public enum TypeItem
{
    None,
    Briefcase,

    Form,

    // first responder
    Notepad,
    Pen,
    PoliceTapeRoll,
    Phone,

    // soco team leader
    CommandPost,

    // soco photographer
    Camera,

    // soco searcher
    EvidenceMarkerItem,
    EvidenceMarkerBody,
    CaseID,

    // soco measurer
    EvidenceRuler,
    TapeMeasure,

    // soco fingerprint specialist
    Chalk,
    Bowl,
    FingerprintPowderBottle,
    FingerprintBrush,
    FingerprintTapeRoll,
    FingerprintTapeLifted,

    Wipes,
    Wipe,
    FingerprintInkingSlab,
    FingerprintInk,
    FingerprintInkRoller,
    FingerprintSpoon,
    FingerprintRecordStrip,

    // soco collector
    SterileSwab,
    SwabDryingRack,
    SwabWrapper,
    SurfaceTapeRoll,
    SurfaceTape,
    TransparentFilm,
    Tweezers,
    EvidencePack, // Ziptop
    EvidenceBox,
    EvidenceSealTapeRoll,

    // ioc or soco team leader part 2
    ItemOfIdentification,

    // evidence custodian
    EvidenceChecklist
}
public enum TypeItemForm
{
    FirstResponder,
    InvestigatorOnCase,
    Sketcher,
    LatentFingerprint,
    ReleaseOfCrimeSceneForm
}
public enum TypeFingerprintBrush
{
    Feather,
    Fiber,
    FlatHead,
    Round
}
public enum TypeFingerprintPowder
{
    None,
    Black,
    Fluorescent,
    Gray,
    White,
    Magnetic,
    Ink
}
public enum TypeRole
{
    None,

    // scene 1
    FirstResponder,
    InvestigatorOnCase,
    SOCOTeamLead,
    Photographer,
    Sketcher,
    Searcher,
    Measurer,
    FingerprintSpecialist,
    Collector,
    EvidenceCustodian,

    // scene 2

}
public enum GameAction
{
    WriteNotepad,
    WriteForm,
    WriteEvidencePackSeal,
    WriteTimeOfArrival,
    WritePulse,
    CheckWristwatch,
    CheckPulse
}

public enum TypeEvidenceMarker
{
    Item,
    Body
}
public enum TypePhoneContact
{
    None,
    DSWD,
    FireMarshal,
    TacticalOperationsCenter,
    UnitDispatchOffice,
    ChiefSOCO,
    BombSquad
}