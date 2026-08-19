namespace TravelCore.Hardening;

/// <summary>
/// P29-R7 deployment / CI/CD / environment config / secret management posture without vendor product.
/// </summary>
public static class HardeningDeploymentSecretsBoundary
{
    public const string SecretsAreRuntimeConfigurationOnly = "Secrets are runtime/deployment configuration only";
    public const string NoSecretsInBusinessTables = "Secrets never persist in business tables";
    public const string DeploymentPostureWithoutCiYamlProduct = "Deployment posture without CI YAML product in T008";
    public const string EnvironmentConfigIsBoundaryOnly = "Environment config posture is boundary-only";
    public const string NoSecretManagerVendorLockIn = "No secret manager vendor lock-in in Hardening module";

    public const bool DeploymentSecretsBoundaryImplemented = true;
    public const bool SecretManagerVendorImplemented = false;
    public const bool CiCdPipelineProductImplemented = false;
    public const bool EnvironmentConfigProductImplemented = false;
}
