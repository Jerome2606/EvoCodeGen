using EvoCodeGen.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace EvoCodeGen.Core.Tests
{
    [TestClass]
    public class GenerationTests
    {
        [TestMethod]
        public async Task ReplaceTokensAsync_Generates_Class_For_Model()
        {
            string template = @"{{for m in model\n~}}\n/*\nGenerated automatically on {{date.now}}\n*/\nusing System;\nusing System.ComponentModel.DataAnnotations;\n\nnamespace [GLOBAL_NAMESPACE].Models\n{\n    public class {{m.entityName}}Entity {{m.baseClass != null ? (\": \" + m.baseClass) : \"\"}}\n    {\n{{for p in m.properties\n~}}\n{{if p.maxLength\n~}}\n        [StringLength({{p.maxLength}})]\n{{~\nend}}\n        public {{p.propertyType | string.capitalize}}{{p.propertyType == \"dateTime\" && p.required == false ? \"?\" : \"\"}} {{p.propertyName}} { get; set; }\n{{~\nend}}\n    }\n}\n{{~\nend}}";

            string json = @"[\n    {\n        \"entityName\": \"Car\",\n        \"entityNamePlural\": \"Cars\",\n        \"databaseTableName\": \"Cars\",\n        \"baseClass\": null,\n        \"primaryKeyType\": \"int\",\n        \"properties\":\n        [\n        { \n            \"propertyType\":\"string\",\n            \"propertyName\": \"Model\",\n            \"minLength\":1,\n            \"maxLength\":100,\n            \"regex\":null,\n            \"required\":true,\n            \"primaryKey\":false\n        },\n        {\n            \"propertyType\":\"dateTime\",\n            \"propertyName\": \"PurchaseDate\",\n            \"required\":false,\n            \"primaryKey\":false\n        }\n        ]\n    }\n]";

            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, template);

            string output = await EvoCodeGenerator.ReplaceTokensAsync("Car", string.Empty, tempFile, json);

            Assert.IsTrue(output.Contains("public class CarEntity"));
            Assert.IsTrue(output.Contains("String Model"));
            Assert.IsTrue(output.Contains("DateTime? PurchaseDate"));

            File.Delete(tempFile);
        }
    }
}
