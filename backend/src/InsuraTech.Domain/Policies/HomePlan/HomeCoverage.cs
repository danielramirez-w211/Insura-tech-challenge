namespace InsuraTech.Domain.Policies.HomePlan;

/// <summary>
/// Tipos de cobertura disponibles para pólizas de hogar (SPEC-012).
/// Serializado en MongoDB como string mediante configuración del serializer BSON.
/// </summary>
public enum HomeCoverage
{
    FireExplosion      = 1,  // Incendio y Explosión (incluida en la prima base)
    Theft              = 2,  // Robo y Hurto
    Plumbing           = 3,  // Fontanería/Daños por Agua
    AestheticDamage    = 4,  // Pintura/Daños Estéticos
    ElectricalDamage   = 5,  // Daños Eléctricos
    GlassBreakage      = 6,  // Rotura de Cristales
    CivilLiability     = 7,  // Responsabilidad Civil
    Uninhabitability   = 8,  // Inhabitabilidad
    LegalDefense       = 9,  // Defensa Jurídica
    HomeAssistance     = 10, // Asistencia en el Hogar
    WaterDamageExpert  = 11  // Daños por Agua/Peritaje
}
