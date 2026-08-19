namespace TravelCore.Evolution;

/// <summary>
/// Post-P29-R8 module extraction / microservice evolution evidence gate.
/// </summary>
public static class EvolutionModuleExtractionBoundary
{
    public const string ModuleExtractionRequiresScaleTeamOpsEvidence =
        "Module extraction requires scale/team/ops evidence";
    public const string MicroserviceExtractionNotDefault = "Microservice extraction is not default scalability path";
    public const string ServiceMeshForbiddenWithoutAdr = "Service mesh forbidden without Accepted ADR";
    public const string ModularMonolithDefaultUntilAdr = "Modular Monolith default until Accepted ADR transition";

    public const bool ModuleExtractionBoundaryImplemented = true;
    public const bool MicroserviceExtractionProductImplemented = false;
    public const bool ServiceMeshProductImplemented = false;
    public const bool ModuleSplitAutomationImplemented = false;
}
