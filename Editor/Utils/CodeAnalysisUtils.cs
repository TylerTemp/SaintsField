using System;
using System.Collections.Generic;
using SaintsField.Editor.Linq;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if SAINTSFIELD_CODE_ANALYSIS
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
#endif

namespace SaintsField.Editor.Utils
{
    public static class CodeAnalysisUtils
    {
        public enum MemberType
        {
            Field,
            Propoerty,
            Method,
        }

        public readonly struct MemberContainer
        {
            public readonly MemberType Type;
            public readonly string Name;
            public readonly IReadOnlyList<string> Arguments;
            public readonly string ReturnType;

            // ReSharper disable once IntroduceOptionalParameters.Global
            public MemberContainer(MemberType type, string name)
            {
                Type = type;
                Name = name;
                Arguments = null;
                ReturnType = null;
            }

            public MemberContainer(string name, IEnumerable<string> arguments, string returnType)
            {
                Type = MemberType.Method;
                Name = name;
                Arguments = arguments.ToArray();
                ReturnType = returnType;
            }

            public override string ToString()
            {
                // ReSharper disable once ConvertSwitchStatementToSwitchExpression
                switch (Type)
                {
                    case MemberType.Field:
                        return $"<Field Name={Name}/>";
                    case MemberType.Propoerty:
                        return $"<Property Name={Name}/>";
                    case MemberType.Method:
                        return $"<Method Name={Name} Arguments=[{string.Join(", ", Arguments)}] ReturnType={ReturnType}/>";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(Type), Type, null);
                }
            }
        }

        public readonly struct ClassContainer
        {
            public readonly string Namespace;
            public readonly string Name;
            public readonly IReadOnlyList<string> BaseTypes;
            public readonly IReadOnlyList<MemberContainer> Members;

            public ClassContainer(string nameSpace, string name, IReadOnlyList<string> baseTypes, IReadOnlyList<MemberContainer> members)
            {
                Namespace = nameSpace;
                Name = name;
                BaseTypes = baseTypes;
                Members = members;
            }

            public override string ToString()
            {
                return $"<Class nameSpace={Namespace} Name={Name} BaseTypes=[{string.Join(", ", BaseTypes)}] Members=[{string.Join(", ", Members)}]/>";
            }
        }

#if SAINTSFIELD_CODE_ANALYSIS
        public static IEnumerable<ClassContainer> Parse(MonoScript ms)
        {
            // MonoScript ms = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/SaintsField/Samples/Scripts/SaintsEditor/Testing/MixLayoutTest.cs");
            string programText = ms.ToString();

            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            foreach (MemberDeclarationSyntax memberDeclarationSyntax in root.Members)
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (memberDeclarationSyntax.Kind())
                {
                    case SyntaxKind.NamespaceDeclaration:
                        foreach (ClassContainer classContainer in ParseNamespace((NamespaceDeclarationSyntax)memberDeclarationSyntax))
                        {
                            yield return classContainer;
                        }
                        break;
                    case SyntaxKind.ClassDeclaration:
                        yield return ParseClass((ClassDeclarationSyntax)memberDeclarationSyntax, null);
                        break;
                }
            }
        }

        private static IEnumerable<ClassContainer> ParseNamespace(NamespaceDeclarationSyntax namespaceDeclarationSyntax)
        {
            string nameSpace = namespaceDeclarationSyntax.Name.ToString();
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (MemberDeclarationSyntax memberDeclarationSyntax in namespaceDeclarationSyntax.Members)
            {
                if (memberDeclarationSyntax.Kind() != SyntaxKind.ClassDeclaration)
                {
                    continue;
                }

                ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)memberDeclarationSyntax;

                yield return ParseClass(classDeclaration, nameSpace);
            }
        }

        private static ClassContainer ParseClass(ClassDeclarationSyntax classDeclaration, string nameSpace)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_CODE_ANALYSIS_DEBUG
            Debug.Log(
                $"[{classDeclaration.Identifier}], Keyword: {classDeclaration.Keyword}, members: {classDeclaration.Members.Count}");
#endif
            List<string> baseTypes = new List<string>();
            // Get base types (class and interfaces) from ClassDeclarationSyntax
            if (classDeclaration.BaseList != null)
            {
                foreach (BaseTypeSyntax baseType in classDeclaration.BaseList.Types)
                {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_CODE_ANALYSIS_DEBUG
                    Debug.Log(
                        $"[{classDeclaration.Identifier}] Base type or interface: {baseType.Type.ToString()}");
#endif
                    baseTypes.Add(baseType.Type.ToString());
                }
            }

            List<MemberContainer> members = new List<MemberContainer>();
            foreach ((MemberDeclarationSyntax memberDeclarationSyntax, int index) in classDeclaration.Members.WithIndex())
            {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_CODE_ANALYSIS_DEBUG
                Debug.Log($"[{index}] [{memberDeclarationSyntax.Kind()}]");
#endif
                switch (memberDeclarationSyntax.Kind())
                {
                    case SyntaxKind.FieldDeclaration:
                        members.Add(ParseField((FieldDeclarationSyntax)memberDeclarationSyntax, index));
                        break;
                    case SyntaxKind.PropertyDeclaration:
                        members.Add(ParseProperty((PropertyDeclarationSyntax)memberDeclarationSyntax, index));
                        break;
                    case SyntaxKind.MethodDeclaration:
                        members.Add(ParseMethod((MethodDeclarationSyntax)memberDeclarationSyntax, index));
                        break;
                }
            }

            return new ClassContainer(nameSpace, classDeclaration.Identifier.Text, baseTypes, members);
        }

        private static MemberContainer ParseField(FieldDeclarationSyntax fieldDeclarationSyntax, int index)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_CODE_ANALYSIS_DEBUG
            Debug.Log(
                $"[{index}]  Field : {fieldDeclarationSyntax.Declaration.Type} {string.Join(", ", fieldDeclarationSyntax.Declaration.Variables.Select(v => v.Identifier.Text))}");
#endif
            return new MemberContainer(MemberType.Field, fieldDeclarationSyntax.Declaration.Variables[0].Identifier.Text);
            // (new MemberContainer(MemberType.Field,
            //     string.Join(", ",
            //         fieldDeclarationSyntax.Declaration.Variables.Select(v =>
            //             v.Identifier.Text)));

            // Debug.Log($"[{index}]  Field : {fieldDeclarationSyntax.Declaration.Type} {string.Join(", ", fieldDeclarationSyntax.Declaration.Variables.Select(v => v.Identifier.Text))}");
            // foreach (AttributeListSyntax attributeList in fieldDeclarationSyntax.AttributeLists)
            // {
            //     foreach (AttributeSyntax attribute in attributeList.Attributes)
            //     {
            //         Debug.Log($"[{index}]    Attribute: {attribute.Name}({string.Join(", ", attribute.ArgumentList?.Arguments.Select(a => a.ToString()) ?? Array.Empty<string>())})");
            //     }
            // }
        }

        private static MemberContainer ParseProperty(PropertyDeclarationSyntax propertyDeclarationSyntax, int index)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_CODE_ANALYSIS_DEBUG
            Debug.Log($"[{index}]  Property : {propertyDeclarationSyntax.Type} {propertyDeclarationSyntax.Identifier.Text}");
#endif
            return new MemberContainer(MemberType.Propoerty, propertyDeclarationSyntax.Identifier.Text);
            // foreach (AttributeListSyntax attributeList in propertyDeclarationSyntax.AttributeLists)
            // {
            //     foreach (AttributeSyntax attribute in attributeList.Attributes)
            //     {
            //         Debug.Log($"[{index}]    Attribute: {attribute.Name}({string.Join(", ", attribute.ArgumentList?.Arguments.Select(a => a.ToString()) ?? Array.Empty<string>())})");
            //     }
            // }
        }

        private static MemberContainer ParseMethod(MethodDeclarationSyntax memberDeclarationSyntax, int index)
        {
#if SAINTSFIELD_DEBUG && SAINTSFIELD_CODE_ANALYSIS_DEBUG
            Debug.Log($"[{index}]  Method : {memberDeclarationSyntax.ReturnType} {memberDeclarationSyntax.Identifier.Text}({string.Join(", ", memberDeclarationSyntax.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier.Text}"))})");
#endif
            return new MemberContainer(
                memberDeclarationSyntax.Identifier.Text,
                memberDeclarationSyntax.ParameterList.Parameters
                    // ReSharper disable once PossibleNullReferenceException
                    .Select(each => each.Type.ToString()),
                memberDeclarationSyntax.ReturnType.ToString());
            // foreach (AttributeListSyntax attributeList in memberDeclarationSyntax.AttributeLists)
            // {
            //     foreach (AttributeSyntax attribute in attributeList.Attributes)
            //     {
            //         Debug.Log($"[{index}]    Attribute: {attribute.Name}({string.Join(", ", attribute.ArgumentList?.Arguments.Select(a => a.ToString()) ?? Array.Empty<string>())})");
            //     }
            // }
        }
#endif
    }
}
