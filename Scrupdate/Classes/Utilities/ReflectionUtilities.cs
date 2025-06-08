// Copyright © 2021-2025 Matan Brightbert
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.




using System;
using System.Collections.Generic;
using System.Reflection;


namespace Scrupdate.Classes.Utilities
{
    public static class ReflectionUtilities
    {
        // Classes /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public readonly struct Field
        {
            public string Name { get; init; }
            public object Value { get; init; }
            public Field() : this("", default) { }
            public Field(string name, object value)
            {
                Name = name;
                Value = value;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        // Methods /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static List<Field> GetStaticFields(Type type)
        {
            return GetStaticFields(type, "");
        }
        public static List<Field> GetStaticFields(Type type, string fieldNameFilter)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (fieldNameFilter == null)
                throw new ArgumentNullException(nameof(fieldNameFilter));
            List<Field> staticFields = new List<Field>();
            foreach (FieldInfo fieldInfo in
                     type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                string fieldName = fieldInfo.Name;
                if (fieldName.Contains(fieldNameFilter))
                {
                    object fieldValue = fieldInfo.GetValue(null);
                    staticFields.Add(new Field(fieldName, fieldValue));
                }
            }
            return staticFields;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
