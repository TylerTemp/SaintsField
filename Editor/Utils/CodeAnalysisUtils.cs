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
        public static IEnumerable<ClassContainer> Parse()
        {
            MonoScript ms = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/SaintsField/Samples/Scripts/SaintsEditor/Testing/MixLayoutTest.cs");
            string programText = ms.ToString();

            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            foreach (MemberDeclarationSyntax memberDeclarationSyntax in root.Members)
            {
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
            //
            // MemberDeclarationSyntax firstMember = root.Members[0];
            // Debug.Log($"The first member is a {firstMember.Kind()}.");
            // NamespaceDeclarationSyntax namespaceDeclaration = (NamespaceDeclarationSyntax)firstMember;
            // Debug.Log($"namespace: {namespaceDeclaration.Name}/{namespaceDeclaration.NamespaceKeyword}");
            //
            // ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)namespaceDeclaration.Members[0];
            // Debug.Log($"[{classDeclaration.Identifier}], Keyword: {classDeclaration.Keyword}, members: {classDeclaration.Members.Count}");
            // // Get base types (class and interfaces) from ClassDeclarationSyntax
            // if (classDeclaration.BaseList != null)
            // {
            //     foreach (BaseTypeSyntax baseType in classDeclaration.BaseList.Types)
            //     {
            //         Debug.Log($"[{classDeclaration.Identifier}] Base type or interface: {baseType.Type.ToString()}");
            //     }
            // }
            //
            // foreach ((MemberDeclarationSyntax memberDeclarationSyntax, int index) in classDeclaration.Members.WithIndex())
            // {
            //     Debug.Log($"[{index}] [{memberDeclarationSyntax.Kind()}]");
            //     switch (memberDeclarationSyntax.Kind())
            //     {
            //         case SyntaxKind.FieldDeclaration:
            //             ParseField((FieldDeclarationSyntax)memberDeclarationSyntax, index);
            //             break;
            //         case SyntaxKind.PropertyDeclaration:
            //             ParseProperty((PropertyDeclarationSyntax)memberDeclarationSyntax, index);
            //             break;
            //         case SyntaxKind.MethodDeclaration:
            //             ParseMethod((MethodDeclarationSyntax)memberDeclarationSyntax, index);
            //             break;
            //     }
            // }

            // MethodDeclarationSyntax mainDeclaration = (MethodDeclarationSyntax)programDeclaration.Members[0];
            //
            // Debug.Log($"The return type of the {mainDeclaration.Identifier} method is {mainDeclaration.ReturnType}.");
            // Debug.Log($"The method has {mainDeclaration.ParameterList.Parameters.Count} parameters.");
            // foreach (ParameterSyntax item in mainDeclaration.ParameterList.Parameters)
            // Debug.Log($"The type of the {item.Identifier} parameter is {item.Type}.");
            // Debug.Log($"The body text of the {mainDeclaration.Identifier} method follows:");
            // Debug.Log(mainDeclaration.Body?.ToFullString());
            //
            // var argsParameter = mainDeclaration.ParameterList.Parameters[0];
        }

        private static IEnumerable<ClassContainer> ParseNamespace(NamespaceDeclarationSyntax namespaceDeclarationSyntax)
        {
            string nameSpace = namespaceDeclarationSyntax.Name.ToString();
            foreach (MemberDeclarationSyntax memberDeclarationSyntax in namespaceDeclarationSyntax.Members)
            {
                if (memberDeclarationSyntax.Kind() != SyntaxKind.ClassDeclaration)
                {
                    continue;
                }

                ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)memberDeclarationSyntax;

                yield return ParseClass(classDeclaration, nameSpace);
                // Debug.Log(
                //     $"[{classDeclaration.Identifier}], Keyword: {classDeclaration.Keyword}, members: {classDeclaration.Members.Count}");
                // List<string> baseTypes = new List<string>();
                // // Get base types (class and interfaces) from ClassDeclarationSyntax
                // if (classDeclaration.BaseList != null)
                // {
                //     foreach (BaseTypeSyntax baseType in classDeclaration.BaseList.Types)
                //     {
                //         Debug.Log(
                //             $"[{classDeclaration.Identifier}] Base type or interface: {baseType.Type.ToString()}");
                //         baseTypes.Add(baseType.Type.ToString());
                //     }
                // }
                //
                // List<MemberContainer> members = new List<MemberContainer>();
                // foreach ((MemberDeclarationSyntax memberDeclarationSyntax2, int index) in classDeclaration
                //              .Members.WithIndex())
                // {
                //     Debug.Log($"[{index}] [{memberDeclarationSyntax2.Kind()}]");
                //     switch (memberDeclarationSyntax2.Kind())
                //     {
                //         case SyntaxKind.FieldDeclaration:
                //             FieldDeclarationSyntax fieldDeclarationSyntax =
                //                 (FieldDeclarationSyntax)memberDeclarationSyntax2;
                //             Debug.Log(
                //                 $"[{index}]  Field : {fieldDeclarationSyntax.Declaration.Type} {string.Join(", ", fieldDeclarationSyntax.Declaration.Variables.Select(v => v.Identifier.Text))}");
                //             members.Add(new MemberContainer(MemberType.Field,
                //                 string.Join(", ",
                //                     fieldDeclarationSyntax.Declaration.Variables.Select(v =>
                //                         v.Identifier.Text))));
                //             break;
                //         case SyntaxKind.PropertyDeclaration:
                //             PropertyDeclarationSyntax propertyDeclarationSyntax =
                //                 (PropertyDeclarationSyntax)memberDeclarationSyntax2;
                //             Debug.Log(
                //                 $"[{index}]  Property : {propertyDeclarationSyntax.Type} {propertyDeclarationSyntax.Identifier.Text}");
                //             members.Add(new MemberContainer(MemberType.Propoerty,
                //                 propertyDeclarationSyntax.Identifier.Text));
                //             break;
                //         case SyntaxKind.MethodDeclaration:
                //             MethodDeclarationSyntax methodDeclarationSyntax =
                //                 (MethodDeclarationSyntax)memberDeclarationSyntax2;
                //             Debug.Log(
                //                 $"[{index}]  Method : {methodDeclarationSyntax.ReturnType} {methodDeclarationSyntax.Identifier.Text}({string.Join(", ", methodDeclarationSyntax.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier.Text}"))})");
                //             members.Add(new MemberContainer(methodDeclarationSyntax.Identifier.Text,
                //                 methodDeclarationSyntax.ParameterList.Parameters
                //                     .Select(p => $"{p.Type} {p.Identifier.Text}").ToList(),
                //                 methodDeclarationSyntax.ReturnType.ToString()));
                //             break;
                //     }
                // }
            }
        }

        private static ClassContainer ParseClass(ClassDeclarationSyntax classDeclaration, string nameSpace)
        {
            Debug.Log(
                $"[{classDeclaration.Identifier}], Keyword: {classDeclaration.Keyword}, members: {classDeclaration.Members.Count}");
            List<string> baseTypes = new List<string>();
            // Get base types (class and interfaces) from ClassDeclarationSyntax
            if (classDeclaration.BaseList != null)
            {
                foreach (BaseTypeSyntax baseType in classDeclaration.BaseList.Types)
                {
                    Debug.Log(
                        $"[{classDeclaration.Identifier}] Base type or interface: {baseType.Type.ToString()}");
                    baseTypes.Add(baseType.Type.ToString());
                }
            }

            List<MemberContainer> members = new List<MemberContainer>();
            foreach ((MemberDeclarationSyntax memberDeclarationSyntax, int index) in classDeclaration.Members.WithIndex())
            {
                Debug.Log($"[{index}] [{memberDeclarationSyntax.Kind()}]");
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
            Debug.Log(
                $"[{index}]  Field : {fieldDeclarationSyntax.Declaration.Type} {string.Join(", ", fieldDeclarationSyntax.Declaration.Variables.Select(v => v.Identifier.Text))}");
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
            Debug.Log($"[{index}]  Property : {propertyDeclarationSyntax.Type} {propertyDeclarationSyntax.Identifier.Text}");
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
            Debug.Log($"[{index}]  Method : {memberDeclarationSyntax.ReturnType} {memberDeclarationSyntax.Identifier.Text}({string.Join(", ", memberDeclarationSyntax.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier.Text}"))})");
            return new MemberContainer(memberDeclarationSyntax.Identifier.Text, memberDeclarationSyntax.ParameterList.Parameters.Select(each => each.Identifier.Text), memberDeclarationSyntax.ReturnType.ToString());
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
