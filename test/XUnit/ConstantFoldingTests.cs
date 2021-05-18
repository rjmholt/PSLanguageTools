using System.Collections.Generic;
using System.Management.Automation.Language;
using PSLanguageTools.CodeAnalysis;
using Xunit;

namespace PSLanguageTools.Test
{
    public class ConstantFoldingTests
    {
        [Theory]
        [MemberData(nameof(SingleScopeTestCases))]
        public void FindsCorrectValues(string program, IReadOnlyDictionary<string, object> expectedValues)
        {
            Ast ast = Parser.ParseInput(program, out Token[] _, out ParseError[] _);

            var visitor = new ConstantFoldingVisitor();
            ast.Visit(visitor);

            Assert.Equal(expectedValues, visitor.GetConstantValues());
        }

        public static TheoryData<string, IReadOnlyDictionary<string, object>> SingleScopeTestCases { get; } = new()
        {
            { "$x = 2", new Dictionary<string, object>{ {"x", 2} } },
            { "$x = 2; $y = 3", new Dictionary<string, object>{ {"x", 2}, {"y", 3} } },
            { "$x = 2; $a = Get-Command; $y = 3", new Dictionary<string, object>{ {"x", 2}, {"y", 3} } },
            { "$x = 2; $y = $x", new Dictionary<string, object>{ {"x", 2}, {"y", 2} } },
            { "$x = 2; $y = $x + 5", new Dictionary<string, object>{ {"x", 2}, {"y", 7} } },
            { "$x = 2; $y = 4*($x + 5)", new Dictionary<string, object>{ {"x", 2}, {"y", 28} } },
            { "$x = 2; $y = 4*$($x + 5)", new Dictionary<string, object>{ {"x", 2}, {"y", 28} } },
            { "$x = 2; $y = $x + 1; $z = 2*$x + 4*$y - 8", new Dictionary<string, object>{ {"x", 2}, {"y", 3}, {"z", 8} } },
            { "$x = 2 -gt 3", new Dictionary<string, object>{ {"x", false} } },
            { "$x = 2 -ge 3", new Dictionary<string, object>{ {"x", false} } },
            { "$x = 2 -lt 3", new Dictionary<string, object>{ {"x", true} } },
            { "$x = 2 -le 3", new Dictionary<string, object>{ {"x", true} } },
            { "$x = 2 -eq 3", new Dictionary<string, object>{ {"x", false} } },
            { "$x = 2 -ne 3", new Dictionary<string, object>{ {"x", true} } },
            { "$x = $true; $y = $false; $z = $x -and $y", new Dictionary<string, object>{ {"x", true}, {"y", false}, {"z", false} } },
            { "$x = $true; $y = $false; $z = $x -or $y", new Dictionary<string, object>{ {"x", true}, {"y", false}, {"z", true} } },
            { "$x = 1; $y = 2; $z = $x -bor $y", new Dictionary<string, object>{ {"x", 1}, {"y", 2}, {"z", 3} } },
            { "$x = 6; $y = 3; $z = $x -band $y", new Dictionary<string, object>{ {"x", 6}, {"y", 3}, {"z", 2} } },
            { "$x = 1; $y = 0; $z = $x -and $y", new Dictionary<string, object>{ {"x", 1}, {"y", 0}, {"z", false} } },
            { "$x = 1,2,3", new Dictionary<string, object>{ {"x", new object[]{ 1, 2, 3 } } } },
            { "$x = @(1, 2, 3)", new Dictionary<string, object>{ {"x", new object[]{ 1, 2, 3 } } } },
            { "$x = @(1; 2; 3)", new Dictionary<string, object>{ {"x", new object[]{ 1, 2, 3 } } } },
            { "$x = 'hello'", new Dictionary<string, object>{ {"x", "hello"} } },
            { "$x = \"hello\"", new Dictionary<string, object>{ {"x", "hello"} } },
            { "$x = 'banana'; $y = \"hello $x\"", new Dictionary<string, object>{ {"x", "banana"}, {"y", "hello banana"} } },
            { "$x = 'banana'; $y = \"hello $x and goodbye\"", new Dictionary<string, object>{ {"x", "banana"}, {"y", "hello banana and goodbye"} } },
            { "$x = 'hello'; $y = \"$x and goodbye\"", new Dictionary<string, object>{ {"x", "hello"}, {"y", "hello and goodbye"} } },
            { "$x = 'hello'; $y = 'goodbye'; $z = \"$x and $y\"", new Dictionary<string, object>{ {"x", "hello"}, {"y", "goodbye"}, {"z", "hello and goodbye"} } },
            { "$x = 'hello'; $y = 'goodbye'; $w = 'greetings'; $z = \"wonderous $w to all and $x and $y to you and you\"", new Dictionary<string, object>{ {"x", "hello"}, {"y", "goodbye"}, {"w", "greetings"}, {"z", "wonderous greetings to all and hello and goodbye to you and you"} } },
            { "$x = \"4 + 17 is $(4 + 17)\"", new Dictionary<string, object>{ {"x", "4 + 17 is 21"} } },
            { "$x = foreach ($i in 1,2,3) { $i + 3 }", new Dictionary<string, object>{ {"x", new object[]{ 4, 5, 6 }} } },
            //{ "$x = 1,2,3 | ForEach-Object { $_ + 2 }", new Dictionary<string, object>{ {"x", new object[]{ 3, 4, 5 }} } },
            //{ "$x = 1,2,3 | ForEach-Object { $PSItem + 2 }", new Dictionary<string, object>{ {"x", new object[]{ 3, 4, 5 }} } },
            //{ "$x = 1,2,3 | Where-Object { $_ -gt 1 }", new Dictionary<string, object>{ {"x", new object[]{ 2, 3 }} } },
        };
    }
}