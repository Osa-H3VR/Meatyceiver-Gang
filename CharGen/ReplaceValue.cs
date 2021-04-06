using System;

namespace CharGen
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ReplaceValue : Attribute
    {
        public readonly bool AllowFloat;
        public readonly bool AsNumbered;

        /// <summary>
        /// Will be able to be used in ReplaceValuesHelper to replace values in a json
        /// </summary>
        /// <param name="allowFloat"></param> Allows number values to be replaced with a float point
        /// <param name="asNumbered"></param> Collection is not replaced as whole, but rather as single elements, same way as if you're just adding new fields like "CollectionName1, CollectionName2" and so on.
        public ReplaceValue(bool allowFloat = false, bool asNumbered = false)
        {
            AllowFloat = allowFloat;
            //TODO! AsNumbered is not really needed, I could do both at the same time
            AsNumbered = asNumbered;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class ReplaceAdditions : Attribute
    {
    }

    public enum ReplaceType
    {
        Number,
        Characters,
        List
    }
}