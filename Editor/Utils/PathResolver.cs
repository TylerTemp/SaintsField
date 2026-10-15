using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SaintsField.SaintsXPathParser;
using SaintsField.SaintsXPathParser.XPathAttribute;
using SaintsField.SaintsXPathParser.XPathFilter;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.Utils
{
    public static partial class PathResolver
    {
        public enum ResourceType
        {
            Folder,
            File,
            Object,
            SceneRoot,
            AssetsRoot,
        }

        public sealed class Node
        {
            public ResourceType ResourceType = ResourceType.Object;
            public object Resource;
            public string FolderPath;
            public Node Container;
            public MemberInfo Member;
            public object Collection;
            public int Index = -1;

            public void WriteBack()
            {
                if (Resource == null || !Resource.GetType().IsValueType || Container == null)
                {
                    return;
                }
                try
                {
                    if (Collection is IList list && Index >= 0)
                    {
                        list[Index] = Resource;
                    }
                    else if (Member is FieldInfo field)
                    {
                        field.SetValue(Container.Resource, Resource);
                    }
                    else if (Member is PropertyInfo property && property.CanWrite)
                    {
                        property.SetValue(Container.Resource, Resource);
                    }
                    Container.WriteBack();
                }
                catch (Exception)
                {
                    // An unwritable branch does not prevent other callback candidates from matching.
                }
            }
        }

        public sealed class Context
        {
            public SerializedProperty Property;
            public MemberInfo Member;
            public object Target;
            public Node Container;
        }


        public static Context CreateContext(SerializedProperty property, MemberInfo member, object target)
        {
            Node container = new Node { Resource = target };
            if (property != null)
            {
                try
                {
                    Node current = new Node { Resource = property.serializedObject.targetObject };
                    (bool _, string[] segments) = SerializedUtils.TrimEndArray(property.propertyPath.Split('.'));
                    // Build the containing instances without reading the decorated value itself.
                    foreach (string segment in segments.Take(segments.Length - 1))
                    {
                        if (segment == "Array")
                        {
                            continue;
                        }
                        current = segment.StartsWith("data[")
                            ? ApplyCollection(current, new XPathCollectionOperation(segment.Substring(5, segment.Length - 6))).FirstOrDefault()
                            : ReadMember(current, new NodeTest { ExactMatch = segment }).FirstOrDefault();
                        if (current == null)
                        {
                            break;
                        }
                    }
                    if (current != null)
                    {
                        container = current;
                        container.Resource = target;
                    }
                }
                catch (Exception)
                {
                    // Non-serialized callback targets still support member resolution.
                }
            }
            return new Context { Property = property, Member = member, Target = target, Container = container };
        }

        public static IReadOnlyList<Node> Resolve(IEnumerable<XPathStep> steps, Context context,
            bool hierarchyNames)
        {
            IReadOnlyList<Node> current = new[]
            {
                hierarchyNames ? new Node { Resource = context.Property?.serializedObject.targetObject ?? context.Target } : context.Container,
            };
            int stepIndex = 0;
            foreach (XPathStep step in steps)
            {
                current = ResolveStep(step, current, context, hierarchyNames, stepIndex).ToArray();
                stepIndex++;
            }
            return current;
        }

        private static IEnumerable<Node> ResolveStep(XPathStep step, IReadOnlyList<Node> input, Context context,
            bool hierarchyNames, int stepIndex)
        {
            IEnumerable<Node> nodes = input;
            bool selector = step.Axis is Axis.Field or Axis.SerializedProperty;
            if (selector)
            {
                if (stepIndex != 0)
                {
                    return Array.Empty<Node>();
                }
                if (step.Axis == Axis.Field)
                {
                    if (context.Member == null)
                    {
                        return Array.Empty<Node>();
                    }
                    nodes = ReadMember(context.Container, new NodeTest { ExactMatch = context.Member.Name });
                    int index = context.Property == null ? -1 : SerializedUtils.PropertyPathIndex(context.Property.propertyPath);
                    if (index >= 0)
                    {
                        nodes = ApplyCollections(nodes, new[] { new XPathCollectionOperation(index.ToString()) });
                    }
                }
                else
                {
                    nodes = context.Property == null ? Array.Empty<Node>() : new[] { new Node { Resource = context.Property } };
                }
            }
            else if (step.Axis == Axis.GameObject)
            {
                nodes = input.Select(ProjectGameObject).Where(node => node != null);
            }

            bool named = !step.NodeTest.NameEmpty;
            bool hierarchy = step.Axis == Axis.GameObject || !selector && step.Axis != Axis.None
                             || hierarchyNames && !selector;
            if (hierarchy && (named || step.SepCount > 1 || step.Axis != Axis.None && !selector))
            {
                XPathStep navigationStep = step;
                if (navigationStep.Axis == Axis.GameObject)
                {
                    navigationStep.Axis = Axis.None;
                }
                List<Node> navigated = new List<Node>();
                foreach (Node node in nodes)
                {
                    Node navigationNode = node;
                    if (step.Axis == Axis.None && node.ResourceType == ResourceType.Object)
                    {
                        if (node.Resource is not GameObject && node.Resource is not Component)
                        {
                            // Ordinary instances have members rather than hierarchy children.
                            if (named && step.SepCount <= 1 && step.NodeTest.ExactMatch != "..")
                            {
                                navigated.AddRange(NavigateObject(step.NodeTest, node));
                            }
                            continue;
                        }
                        navigationNode = ProjectGameObject(node);
                        if (navigationNode == null)
                        {
                            continue;
                        }
                    }
                    navigated.AddRange(NavigatePath(navigationStep, new[] { navigationNode }, context));
                }
                nodes = navigated;
            }
            else if (named)
            {
                List<Node> navigated = new List<Node>();
                foreach (Node node in nodes)
                {
                    navigated.AddRange(NavigateObject(step.NodeTest, node));
                }
                nodes = navigated;
            }

            if (step.Attr != null)
            {
                List<Node> attributed = new List<Node>();
                foreach (Node node in nodes)
                {
                    attributed.AddRange(ResolveAttribute(step.Attr, node));
                }
                nodes = attributed;
            }
            nodes = ApplyCollections(nodes, step.CollectionOperations);
            return ApplyPredicates(nodes, step.Predicates);
        }

        private static Node ProjectGameObject(Node node)
        {
            try
            {
                GameObject gameObject = node.Resource is GameObject go ? go : (node.Resource as Component)?.gameObject;
                return gameObject ? new Node { Resource = gameObject, Container = node.Container, Member = node.Member } : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static IEnumerable<Node> NavigateObject(NodeTest test, Node node)
        {
            if (test.ExactMatch == ".")
            {
                return new[] { node };
            }
            if (test.ExactMatch == "..")
            {
                Transform transform = node.Resource is GameObject go ? go.transform : (node.Resource as Component)?.transform;
                if (transform)
                {
                    return transform.parent ? new[] { new Node { Resource = transform.parent.gameObject } } : Array.Empty<Node>();
                }
                return node.Container == null ? Array.Empty<Node>() : new[] { node.Container };
            }
            return ReadMember(node, test);
        }

        public static IEnumerable<Node> ReadMember(Node node, NodeTest test)
        {
            if (node.Resource == null)
            {
                yield break;
            }
            foreach (Type type in ReflectUtils.GetSelfAndBaseTypesFromInstance(node.Resource))
            {
                List<MemberInfo> members = new List<MemberInfo>();
                if (test.ExactMatch != null)
                {
                    FieldInfo field = type.GetField(test.ExactMatch, ReflectUtils.FindTargetBindAttr)
                                      ?? type.GetField($"<{test.ExactMatch}>k__BackingField", ReflectUtils.FindTargetBindAttr);
                    if (field != null)
                    {
                        members.Add(field);
                    }
                    else
                    {
                        members.AddRange(type.GetProperties(ReflectUtils.FindTargetBindAttr).Where(prop => prop.Name == test.ExactMatch));
                        if (members.Count == 0)
                        {
                            members.AddRange(type.GetMethods(ReflectUtils.FindTargetBindAttr).Where(method => method.Name == test.ExactMatch));
                        }
                    }
                }
                else
                {
                    members.AddRange(type.GetMembers(ReflectUtils.FindTargetBindAttr)
                        .Where(member => (member is FieldInfo || member is PropertyInfo || member is MethodInfo)
                                         && NodeTestMatch.NodeMatch(member.Name, test)));
                }
                foreach (MemberInfo member in members)
                {
                    object value;
                    try
                    {
                        value = member switch
                        {
                            FieldInfo field => field.GetValue(node.Resource),
                            PropertyInfo property => property.GetValue(node.Resource),
                            _ => member,
                        };
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    yield return new Node { Resource = value, Container = node, Member = member };
                }
                if (test.ExactMatch != null && members.Any(member => !(member is MethodInfo)))
                {
                    yield break;
                }
            }
        }

        public static IEnumerable<Node> ResolveAttribute(XPathAttrBase attr, Node node)
        {
            if (node.ResourceType == ResourceType.File && (attr is XPathAttrFakeEval || attr is XPathAttrAs || attr is XPathAttrGetComponents))
            {
                string path = string.IsNullOrEmpty(node.FolderPath) ? (string)node.Resource : $"{node.FolderPath}/{node.Resource}";
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (!asset)
                {
                    return Array.Empty<Node>();
                }
                node = new Node { Resource = asset, Container = node.Container, Member = node.Member };
            }
            switch (attr)
            {
                case XPathAttrGetComponents components:
                {
                    IEnumerable<Component> matches = GetComponents(node, components.TypeName) ?? Array.Empty<Component>();
                    if (components.FirstOnly)
                    {
                        matches = matches.Take(1);
                    }
                    return matches.Select(component => new Node { Resource = component, Container = node });
                }
                case XPathAttrFakeEval expression:
                    return EvaluateExpression(expression, node);
                case XPathAttrAs typeTest:
                {
                    bool matches = node.Resource != null && FindTypes(typeTest.TypeName).Any(type => type.IsInstanceOfType(node.Resource));
                    return matches != typeTest.Negate ? new[] { node } : Array.Empty<Node>();
                }
                case XPathAttrCollection collection:
                    return ApplyCollections(ResolveAttribute(collection.Inner, node), collection.Operations);
                default:
                    return GetValuesFromAttr(attr, new[] { node });
            }
        }

        public static IEnumerable<Node> EvaluateExpression(XPathAttrFakeEval expression, Node node)
        {
            IEnumerable<Node> nodes = new[] { node };
            foreach (XPathAttrFakeEval.ExecuteFragment fragment in expression.ExecuteFragments)
            {
                List<Node> evaluated = new List<Node>();
                foreach (Node current in nodes)
                {
                    evaluated.AddRange(EvaluateFragment(fragment, current));
                }
                nodes = evaluated;
            }
            return nodes;
        }

        private static IEnumerable<Node> EvaluateFragment(XPathAttrFakeEval.ExecuteFragment fragment, Node node)
        {
            IEnumerable<Node> results;
            switch (fragment.ExecuteType)
            {
                case XPathAttrFakeEval.ExecuteType.GetComponents:
                {
                    Component[] components = GetComponents(node, fragment.ExecuteString);
                    if (components == null)
                    {
                        return Array.Empty<Node>();
                    }
                    results = new[] { new Node { Resource = components, Container = node } };
                    break;
                }
                case XPathAttrFakeEval.ExecuteType.FieldOrProperty:
                    results = ReadMember(node, new NodeTest { ExactMatch = fragment.ExecuteString });
                    break;
                case XPathAttrFakeEval.ExecuteType.Method:
                    results = ReadMember(node, new NodeTest { ExactMatch = fragment.ExecuteString })
                        .Where(result => result.Resource is MethodInfo)
                        .SelectMany(InvokeNoParams);
                    break;
                default:
                    return Array.Empty<Node>();
            }
            return ApplyCollections(results, fragment.CollectionOperations);
        }

        private static Component[] GetComponents(Node node, string typeName)
        {
            GameObject go = node.Resource as GameObject ?? (node.Resource as Component)?.gameObject;
            if (!go)
            {
                return null;
            }
            Component[] components = go.GetComponents<Component>().Where(component => component).ToArray();
            if (!string.IsNullOrEmpty(typeName))
            {
                IReadOnlyList<Type> types = FindTypes(typeName);
                components = components.Where(component => types.Any(type => type.IsInstanceOfType(component))).ToArray();
            }
            return components;
        }

        private static IEnumerable<Node> InvokeNoParams(Node node)
        {
            object value;
            try
            {
                MethodInfo method = (MethodInfo)node.Resource;
                value = method.Invoke(method.IsStatic ? null : node.Container?.Resource, Array.Empty<object>());
                node.Container?.WriteBack();
            }
            catch (Exception)
            {
                yield break;
            }
            yield return new Node { Resource = value, Container = node.Container };
        }

        private static IEnumerable<Node> ApplyCollections(IEnumerable<Node> nodes, IReadOnlyList<XPathCollectionOperation> operations)
        {
            if (operations != null)
            {
                foreach (XPathCollectionOperation operation in operations)
                {
                    List<Node> collected = new List<Node>();
                    foreach (Node node in nodes)
                    {
                        collected.AddRange(ApplyCollection(node, operation));
                    }
                    nodes = collected;
                }
            }
            return nodes;
        }

        private static IEnumerable<Node> ApplyCollection(Node node, XPathCollectionOperation operation)
        {
            string content = operation.Content;
            if (content.StartsWith("index()"))
            {
                IReadOnlyList<IReadOnlyList<XPathPredicate>> predicates = XPathBracketParser.ParseFilter("[" + content + "]")
                    .Select(group => (IReadOnlyList<XPathPredicate>)group.Select(item => new XPathPredicate
                    {
                        Attr = item.attrBase,
                        FilterComparer = item.filterComparerBase,
                    }).ToArray()).ToArray();
                return ApplyPredicates(ApplyCollection(node, new XPathCollectionOperation("*")), predicates);
            }
            if (content == "*" && node.Resource is IEnumerable enumerable and not string)
            {
                List<Node> expanded = new List<Node>();
                try
                {
                    int index = 0;
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (object value in enumerable)
                    {
                        expanded.Add(CollectionElement(node, value, index++));
                    }
                }
                catch (Exception)
                {
                    return Array.Empty<Node>();
                }
                return expanded;
            }
            object result;
            int elementIndex = -1;
            try
            {
                if (node.Resource is IList list && (int.TryParse(content, out elementIndex) || content == "last()"))
                {
                    if (content == "last()")
                    {
                        elementIndex = list.Count - 1;
                    }
                    else if (elementIndex < 0)
                    {
                        elementIndex += list.Count;
                    }
                    result = list[elementIndex];
                }
                else if (node.Resource is IEnumerable && !(node.Resource is string)
                         && (int.TryParse(content, out elementIndex) || content == "last()"))
                {
                    Node[] elements = ApplyCollection(node, new XPathCollectionOperation("*")).ToArray();
                    if (content == "last()")
                    {
                        elementIndex = elements.Length - 1;
                    }
                    else if (elementIndex < 0)
                    {
                        elementIndex += elements.Length;
                    }
                    return new[] { elements[elementIndex] };
                }
                else if (node.Resource is IDictionary dictionary && content.Length >= 2)
                {
                    object key = content.Substring(1, content.Length - 2);
                    if (!dictionary.Contains(key))
                    {
                        return Array.Empty<Node>();
                    }
                    result = dictionary[key];
                }
                else
                {
                    return Array.Empty<Node>();
                }
            }
            catch (Exception)
            {
                return Array.Empty<Node>();
            }
            return new[] { CollectionElement(node, result, elementIndex) };
        }

        private static Node CollectionElement(Node collection, object value, int index)
        {
            return new Node
            {
                Resource = value,
                Container = collection.Container,
                Member = collection.Member,
                Collection = collection.Resource,
                Index = index,
            };
        }

        private static IEnumerable<Node> ApplyPredicates(IEnumerable<Node> nodes,
            IReadOnlyList<IReadOnlyList<XPathPredicate>> predicates)
        {
            if (predicates == null)
            {
                return nodes;
            }
            foreach (IReadOnlyList<XPathPredicate> alternatives in predicates)
            {
                Node[] snapshot = nodes.ToArray();
                nodes = snapshot.Where((node, index) => alternatives.Any(predicate =>
                {
                    if (predicate.Attr is XPathAttrIndex position)
                    {
                        if (!position.Last && predicate.FilterComparer is FilterComparerInt integer
                            && integer.FilterComparer == FilterComparer.Equal && integer.Value < 0)
                        {
                            return index == snapshot.Length + integer.Value;
                        }
                        return position.Last ? index == snapshot.Length - 1 : Match(new Node { Resource = index }, predicate.FilterComparer);
                    }
                    IEnumerable<Node> results = ResolveAttribute(predicate.Attr, node);
                    if (predicate.Attr is XPathAttrAs && predicate.FilterComparer is FilterComparerTruly)
                    {
                        return results.Any();
                    }
                    return results.Any(result => Match(result, predicate.FilterComparer));
                })).ToArray();
            }
            return nodes;
        }

        private static bool Match(Node node, FilterComparerBase comparer)
        {
            object value = node.Resource;
            switch (comparer)
            {
                case FilterComparerTruly _:
                    return node.ResourceType == ResourceType.File ? File.Exists((string)value) : ReflectUtils.Truly(value);
                case FilterComparerInt integer:
                    return value is IComparable comparable && integer.CompareToComparable(comparable);
                case FilterComparerString text:
                    return value is bool boolean ? text.CompareToBool(boolean)
                        : value is string str && text.CompareToString(str);
                case FilterComparerBasePath path:
                    return value is string source && path.CompareToString(source);
                default:
                    return false;
            }
        }

        private static Dictionary<Assembly, Type[]> _cachedAssemblyTypes;
        private static readonly Dictionary<string, IReadOnlyList<Type>> CachedNameToType = new Dictionary<string, IReadOnlyList<Type>>();

        public static IReadOnlyList<Type> FindTypes(string shortOrQualifiedName)
        {
            if (CachedNameToType.TryGetValue(shortOrQualifiedName, out IReadOnlyList<Type> cachedTypes))
            {
                return cachedTypes;
            }

            bool containDot = shortOrQualifiedName.Contains(".");
            string shortName = containDot
                ? shortOrQualifiedName.Split('.').Last()
                : shortOrQualifiedName;

            _cachedAssemblyTypes ??= Util.GetAssemblies()
                .ToDictionary(assembly => assembly, Util.GetAssemblyTypesSafe);

            List<Type> exactlyFullMatchedTypes = new List<Type>();
            List<Type> unityEngineTypes = new List<Type>();
            List<Type> unityTypes = new List<Type>();
            List<Type> otherTypes = new List<Type>();
            foreach (KeyValuePair<Assembly, Type[]> kv in _cachedAssemblyTypes)
            {
                foreach (Type t in kv.Value)
                {
                    string fullNameT = t.FullName;
                    if (containDot && fullNameT == shortOrQualifiedName)
                    {
                        exactlyFullMatchedTypes.Add(t);
                    }
                    else if(t.Name == shortName
                            || fullNameT == shortOrQualifiedName
                            || fullNameT?.EndsWith("." + shortName, StringComparison.Ordinal) == true)
                    {
                        if (!string.IsNullOrEmpty(fullNameT))
                        {
                            if (fullNameT.StartsWith("UnityEngine."))
                            {
                                unityEngineTypes.Add(t);
                            }
                            else if (fullNameT.StartsWith("Unity."))
                            {
                                unityTypes.Add(t);
                            }
                            else
                            {
                                otherTypes.Add(t);
                            }
                        }
                        else
                        {
                            otherTypes.Add(t);
                        }
                    }
                }
            }

            exactlyFullMatchedTypes.AddRange(unityEngineTypes);
            exactlyFullMatchedTypes.AddRange(unityTypes);
            exactlyFullMatchedTypes.AddRange(otherTypes);

            return CachedNameToType[shortOrQualifiedName] = exactlyFullMatchedTypes;
        }

    }
}
