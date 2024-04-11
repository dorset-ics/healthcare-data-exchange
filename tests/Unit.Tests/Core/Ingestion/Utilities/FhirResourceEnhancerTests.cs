using Core.Common.Abstractions.Services;
using Core.Common.Utilities;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Core.Ingestion.Utilities
{
    public class FhirResourceEnhancerTests
    {
        private readonly FhirJsonParser fhirJsonParser;
        private readonly FhirResourceEnhancer _fhirResourceEnhancer;
        private readonly ITerminologyService _terminologyService;
        private readonly ILogger<FhirResourceEnhancer> _loggerMock;
        public FhirResourceEnhancerTests()
        {
            fhirJsonParser = new FhirJsonParser();
            _terminologyService = Substitute.For<ITerminologyService>();
            _loggerMock = Substitute.For<ILogger<FhirResourceEnhancer>>();
            _fhirResourceEnhancer = new FhirResourceEnhancer(_terminologyService, _loggerMock);
        }

        [Fact]
        public void GivenBundle_WhenNoDisplay_ThenAddDisplay()
        {
            var inputJsonObject =
            """
            {
            "resourceType": "Bundle",
            "id": "1",
            "entry": [
                {
                "fullUrl": "2",
                "resource": {
                    "resourceType": "Encounter",
                    "type": [
                    {
                        "coding": [
                        {
                            "code": "319981000000104",
                            "system": "http://snomed.info/sct"
                        }
                        ]
                    }
                    ],
                    "reasonCode": [
                    {
                        "coding": [
                        {
                            "code": "182813001",
                            "system": "http://snomed.info/sct"
                        }
                        ]
                    }
                    ]
                }
                },
                {
                "fullUrl": "3",
                "resource": {
                    "resourceType": "Condition",
                    "code": {
                    "coding": [
                        {
                        "code": "182813001",
                        "system": "http://snomed.info/sct"
                        }
                    ]
                    }
                }
                }
            ]
            }
            """;

            var code1 = "319981000000104";
            var code1Display = "Seen in urgent care centre (finding)";
            _terminologyService.GetSnomedDisplay(code1).Returns(code1Display);


            var code2 = "182813001";
            var code2Display = "Emergency treatment (procedure)";
            _terminologyService.GetSnomedDisplay(code2).Returns(code2Display);

            var inputBundle = fhirJsonParser.Parse<Bundle>(inputJsonObject);
            var actualBundle = _fhirResourceEnhancer.Enrichment(inputBundle);

            var expectedJson =
            """
            {
            "resourceType": "Bundle",
            "id": "1",
            "entry": [
                {
                "fullUrl": "2",
                "resource": {
                    "resourceType": "Encounter",
                    "type": [
                    {
                        "coding": [
                        {
                            "code": "319981000000104",
                            "system": "http://snomed.info/sct",
                            "display": "Seen in urgent care centre (finding)"
                        }
                        ]
                    }
                    ],
                    "reasonCode": [
                    {
                        "coding": [
                        {
                            "code": "182813001",
                            "system": "http://snomed.info/sct",
                            "display": "Emergency treatment (procedure)"
                        }
                        ]
                    }
                    ]
                }
                },
                {
                "fullUrl": "3",
                "resource": {
                    "resourceType": "Condition",
                    "code": {
                    "coding": [
                        {
                        "code": "182813001",
                        "system": "http://snomed.info/sct",
                        "display": "Emergency treatment (procedure)"
                        }
                    ]
                    }
                }
                }
            ]
            }
            """;
            var expectedBundle = fhirJsonParser.Parse<Bundle>(expectedJson);

            actualBundle.Value.IsExactly(expectedBundle).ShouldBeTrue();
        }

        [Fact]
        public void GivenBundle_WhenDisplay_ThenNoChange()
        {
            var inputJsonObject =
            """
            {
            "resourceType": "Bundle",
            "id": "1",
            "entry": [
                {
                "fullUrl": "2",
                "resource": {
                    "resourceType": "Encounter",
                    "type": [
                    {
                        "coding": [
                        {
                            "code": "319981000000104",
                            "system": "http://snomed.info/sct",
                            "display": "Seen in urgent care centre (finding)"
                        }
                        ]
                    }
                    ],
                    "reasonCode": [
                    {
                        "coding": [
                        {
                            "code": "182813001",
                            "system": "on snomed system",
                            "display": "Emergency treatment (procedure)"
                        }
                        ]
                    }
                    ]
                }
                },
                {
                "fullUrl": "2",
                "resource": {
                    "resourceType": "Condition",
                    "code": {
                    "coding": [
                        {
                        "code": "182813001",
                        "system": "http://snomed.info/sct",
                        "display": "Emergency treatment (procedure)"
                        }
                    ]
                    }
                }
                }
            ]
            }
            """;

            var inputBundle = fhirJsonParser.Parse<Bundle>(inputJsonObject);
            var actualBundle = _fhirResourceEnhancer.Enrichment(inputBundle);

            actualBundle.Value.IsExactly(inputBundle).ShouldBeTrue();
        }

        [Fact]
        public void GivenResource_WhenNoDisplay_ThenAddDisplay()
        {
            var inputJsonObject =
            """
            {
            "resourceType": "Observation",
            "code": {
                "coding": [
                {
                    "system": "http://snomed.info/sct",
                    "code": "271649006"
                },
                {
                    "system": "http://snomed.info/sct",
                    "code": "386725007"
                },
                {
                    "system": "http://snomed.info/sct",
                    "code": "A code that does not exist"
                }
                ]
            }
            }
            """;

            var code1 = "271649006";
            var code1Display = "Systolic blood pressure (observable entity)";
            _terminologyService.GetSnomedDisplay(code1).Returns(code1Display);


            var code2 = "386725007";
            var code2Display = "Body temperature (observable entity)";
            _terminologyService.GetSnomedDisplay(code2).Returns(code2Display);

            var inputResource = fhirJsonParser.Parse<Resource>(inputJsonObject);
            var actualResource = _fhirResourceEnhancer.Enrichment(inputResource);

            var expectedJson =
            """
            {
            "resourceType": "Observation",
            "code": {
                "coding": [
                {
                    "system": "http://snomed.info/sct",
                    "code": "271649006",
                    "display": "Systolic blood pressure (observable entity)"
                },
                {
                    "system": "http://snomed.info/sct",
                    "code": "386725007",
                    "display": "Body temperature (observable entity)"
                },
                {
                    "system": "http://snomed.info/sct",
                    "code": "A code that does not exist"
                }
                ]
            }
            }
            """;

            var expectedResource = fhirJsonParser.Parse<Resource>(expectedJson);

            actualResource.Value.IsExactly(expectedResource).ShouldBeTrue();
        }

        [Fact]
        public void GivenResource_WhenDisplayPresent_ThenNoChange()
        {
            var inputJsonObject =
            """
            {
            "resourceType": "Observation",
            "code": {
                "coding": [
                {
                    "system": "http://snomed.info/sct",
                    "code": "271649006",
                    "display": "Systolic blood pressure (observable entity)"
                },
                {
                    "system": "http://snomed.info/sct",
                    "code": "386725007",
                    "display": "Body temperature (observable entity)"
                },
                {
                    "system": "non snomed system",
                    "code": "1234"
                }
                ]
            }
            }
            """;

            var inputResource = fhirJsonParser.Parse<Resource>(inputJsonObject);
            var actualResource = _fhirResourceEnhancer.Enrichment(inputResource);

            actualResource.Value.IsExactly(inputResource).ShouldBeTrue();
        }

        [Fact]
        public void GivenResource_WhenNoMatchOnCode_ThenNoChange()
        {
            var inputJsonObject =
            """
            {
            "resourceType": "Observation",
            "code": {
                "coding": [
                {
                    "system": "http://snomed.info/sct",
                    "code": "1234abcd"
                }
                ]
            }
            }
            """;

            var inputResource = fhirJsonParser.Parse<Resource>(inputJsonObject);
            var actualResource = _fhirResourceEnhancer.Enrichment(inputResource);

            actualResource.Value.IsExactly(inputResource).ShouldBeTrue();
        }
    }
}
