using System.Xml.Linq;
using System;
using OfficeOpenXml;

namespace EPPLus.MultiHeader.Test
{
    public class BasicReportRender
    {

        public BasicReportRender()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [Fact]
        public void Write2Rows()
        {
            var people = new List<Person>
            {
                new Person("M�diamass","Large", DateTime.Parse("2017/05/28")),
                new Person("Aim�e","Bateson", DateTime.Parse("1958/06/07"))
            };
            var xls = new ExcelPackage();

            var report = new MultiHeaderReport<Person>(xls, "People");
            report.GenerateReport(people);

            var sheet = xls.Workbook.Worksheets["People"];
            Assert.Equal(4, sheet.Dimension.End.Column);
            Assert.Equal(3, sheet.Dimension.End.Row);
            Assert.Equal(nameof(Person.Age), sheet.Cells[1, 4].GetValue<string>());
            Assert.Equal("Bateson", sheet.Cells[3, 2].GetValue<string>());
        }
    }
}