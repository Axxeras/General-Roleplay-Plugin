namespace GRPP.Extensions;

using System;
using System.Collections.Generic;
using System.Reflection;
using API.Features.Department;

public static class TypeScaffold
{
    public static object? Scaffold(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return null; // a generic type is essentially any type that has a type parameter - smt that'll be filled in later.
                                                                                                      // Such as a Nullable<int> [ or `int?` with modern syntax], a List<string>, a Dictionary<int, string>, etc.
                                                                                                      // -- booleans, ints, etc. that are not nullable will not be added to this
                                                                                                      // MAJOR NOTE: Nullable<string> does NOT exist!! The compiler will refuse it. `string?` is the same as `string`, the 
                                                                                                      // "?" just tells the compiler 'aye, warn me if I forget to null check this!" :)
        if (type == typeof(string)) return null; // if it's a string, leave it alone!!
        if (type.IsGenericType) // this is a check for if it's a generic type - like a list, or a Nullable<>!!! [boolean, ofc]
        {
            var genericDef = type.GetGenericTypeDefinition(); // takes away the string from List<string>, and comes back with List<T> !!! :DD || Dictionary<string, string> becomes Dictionary<T,T> -- unfilled types !!!
            if (genericDef == typeof(List<>) || genericDef == typeof(Dictionary<,>)) // checks for Lists and Dictionaries !!!!!
                return null; // returns null if list or dictionary :D
        }
        if (type.IsValueType) // value types are ints, bool, float, etc. ! these are separate from Reference types, which are things like classes !!!!!
            return Activator.CreateInstance(type); // gives default value, such as (int)0 or (bool)false!

        if (type.Assembly != typeof(DepartmentInfo).Assembly) // if the type's assembly is not yamlhandler's assembly, get outta here!!!
            return null;

        var instance = Activator.CreateInstance(type); // get instance of type
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) // for each property in type.GetProperties (returns PropertyInfo objects!!! one per property, which is why we're in a foreach loop.)
        {
            if (!property.CanWrite) continue; // if we can't write this property?! get outta here! if not, let's continue **IN** the foreach loop. [if there's no setter!]

            var value = Scaffold(property.PropertyType); // set value to Scaffold(property.PropertyType) - which recurses through the property (and, since we're in a foreach loop, properties)'s propertytype and then finallyyy (next line)

            property.SetValue(instance, value); // finally sets the value to instance (which is CreateInstance(type) from earlier,  value -- which is the stripped & fixed property!!!!!!!!!!!!
        }

        return instance; // then we hand it alll over :D
     }
}