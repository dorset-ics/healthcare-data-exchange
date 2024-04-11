using Core.Common.Models;

namespace Unit.Tests.Core.Common.Models;

public class TemplateInfoTests
{
    [Fact]
    public void Test_Name_ShouldGenerateValidName()
    {
        var templateInfo = new TemplateInfo("Uhd", "AgyleEd", "Hl7", "A01");
        templateInfo.Name.ShouldBe("uhd_agyleed_hl7_a01");
    }

    [Fact]
    public void Test_ForPdsMesh_ShouldReturnValidName()
    {
        var templateInfo = TemplateInfo.ForPdsMeshPatient();
        templateInfo.Name.ShouldBe("x26_pds-mesh_json_patient");
    }
}