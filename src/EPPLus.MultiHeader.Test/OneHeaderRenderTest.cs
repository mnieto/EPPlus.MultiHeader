using System.Xml.Linq;
using System;
using OfficeOpenXml;
using NuGet.Frameworks;

namespace EPPLus.MultiHeader.Test
{
    public class OneHeaderRenderTest
    {
        private int maxColumns;
        public OneHeaderRenderTest()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            maxColumns = typeof(Person).GetProperties().Length;
        }

        private List<Person> BuildPeopleList()
        {
            return new List<Person>
            {
                new Person("M�diamass","Large", DateTime.Parse("2017/05/28"), null, null),
                new Person("Aim�e","Bateson", DateTime.Parse("1958/06/07"), 2, new Uri("https://github.com/"))
            };
        }

        [Fact]
        public void Write2Rows()
        {
            var people = BuildPeopleList();
            using var xls = new ExcelPackage();

            var report = new MultiHeaderReport<Person>(xls, "People");
            report.GenerateReport(people);

            var sheet = xls.Workbook.Worksheets["People"];
            Assert.Equal(maxColumns, sheet.Dimension.End.Column);
            Assert.Equal(3, sheet.Dimension.End.Row);
            Assert.Equal(nameof(Person.NumOfComputers), sheet.Cells[1, 4].GetValue<string>());
            Assert.Equal("Bateson", sheet.GetValue<string>(3, 2));
            Assert.Null(sheet.GetValue(2, 4));
            Assert.Equal(2, sheet.GetValue<int>(3, 4));
            Assert.Equal("https://github.com/", sheet.GetValue(3, 5).ToString());
        }

        [Fact]
        public void Config_SetupOrder_ColumnsAreOrdered()
        {
            var people = BuildPeopleList();
            using var xls = new ExcelPackage();
            var report = new MultiHeaderReport<Person>(xls, "People");
            report.Configure(options => options
                .AddColumn(x => x.NumOfComputers, 1)
            ).GenerateReport(people);

            var sheet = xls.Workbook.Worksheets["People"];
            Assert.Equal(nameof(Person.NumOfComputers), sheet.GetValue<string>(1, 1));
            Assert.Equal(nameof(Person.Name), sheet.GetValue<string>(1, 2));
        }

        [Fact]
        public void Config_IgnoredColumns_AreNotInTheList()
        {
            var people = BuildPeopleList();
            using var xls = new ExcelPackage();
            var report = new MultiHeaderReport<Person>(xls, "People");
            report.Configure(options => options
                .AddColumn(x => x.Surname, 1)
                .IgnoreColumn(x => x.NumOfComputers)
            ).GenerateReport(people);

            var sheet = xls.Workbook.Worksheets["People"];
            Assert.Equal(4, sheet.Dimension.End.Column);
        }

        [Fact]
        public void HiddenColumns_AreRendered_AsHidden()
        {
            var people = BuildPeopleList();
            using var xls = new ExcelPackage();
            var report = new MultiHeaderReport<Person>(xls, "People");
            report.Configure(options => options
                .AddColumn(x => x.NumOfComputers, hidden: true)
            ).GenerateReport(people);

            var sheet = xls.Workbook.Worksheets["People"];
            Assert.True(sheet.Column(4).Hidden);
        }

        [Fact]
        public void HyperLinkColumns_UseAntherColumnTo_BuildTheLink()
        {
            var people = BuildPeopleList();
            using var xls = new ExcelPackage();
            var report = new MultiHeaderReport<Person>(xls, "People");
            report.Configure(options => options
                .AddHyperLinkColumn(x => x.Name, x => x.Profile)
                .IgnoreColumn(x => x.Profile)
            ).GenerateReport(people);

            var sheet = xls.Workbook.Worksheets["People"];
            Assert.True(sheet.Cells[3, 1].Hyperlink != null);
        }

        [Fact]
        public void FormulaColumns_Write_Formulas()
        {
            var people = BuildPeopleList();
            using var xls = new ExcelPackage();

            var report = new MultiHeaderReport<Person>(xls, "People");
            report.Configure(options => options
                .AddColumn(x => x.Name, 1)
                .AddColumn(x => x.Surname, 2)
                .AddFormula("CompleteName", "CONCATENATE(B2,\", \",A2)", 3)
            ).GenerateReport(people);

            var sheet = xls.Workbook.Worksheets["People"];
            Assert.Equal("Bateson, Aim�e", sheet.GetValue<string>(3, 3));
        }

        [Fact]
        public void ExpressionColumns_Write_ExpressionResults()
        {
            var people = BuildPeopleList();
            using var xls = new ExcelPackage();

            var report = new MultiHeaderReport<Person>(xls, "People");
            report.Configure(options => options
                .AddColumn(x => x.Name, 1)
                .AddColumn(x => x.Surname, 2)
                .AddExpression("Initials", x => string.Concat(x.Name[0], '.', x.Surname[0], '.'), 3)
            ).GenerateReport(people);

            var sheet = xls.Workbook.Worksheets["People"];
            Assert.Equal("A.B.", sheet.GetValue<string>(3, 3));
        }

    }
}