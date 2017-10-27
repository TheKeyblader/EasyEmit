using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace EasyEmit.Creator
{
    public class AssemblyCreator
    {
        private AssemblyBuilder assemblyBuilder;
        private ModuleBuilder moduleBuilder;
        public string Name { get { return name; } }
        private string name;

        private IEnumerable<TypeCreator> typeCreators;
        private IEnumerable<EnumCreator> enumCreators;

        public AssemblyCreator(string name)
        {
            AssemblyName assemblyName = new AssemblyName(name);
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("hey");
            this.name = name;
            typeCreators = new List<TypeCreator>();
            enumCreators = new List<EnumCreator>();

        }

        public TypeCreator CreateClass(string name,TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.Class)
        {
            VerificationName(name);
            TypeCreator type = new TypeCreator(name, moduleBuilder, typeAttributes);
            typeCreators.Append(type);
            return type;
        }
        public EnumCreator CreateEnum(string name,TypeAttributes typeAttributes = TypeAttributes.Public)
        {
            VerificationName(name);
            if (typeAttributes.HasFlag(TypeAttributes.Interface))
            {
                throw new Exception("An enum cant have interface attribute");
            }
            EnumCreator @enum = new EnumCreator(name, moduleBuilder, typeAttributes);
            enumCreators.Append(@enum);
            return @enum;
        }
        private void VerificationName(string name)
        {
            if(typeCreators.Any(t => t.Name == name))
            {
                throw new Exception("This name is already used in the class " + name);
            }
            if(enumCreators.Any(e => e.Name == name))
            {
                throw new Exception("This name is already used in the enum " + name);
            }
        }
    }
}