using System;
using System.Reflection.Emit;
using System.Reflection;
using EasyEmit.Creator;
namespace EmitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var lol = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName("lol"), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = lol.DefineDynamicModule("hey");
            EnumCreator enumCreator = new EnumCreator("lol", moduleBuilder);
            enumCreator.AddEntry("lol", 1);
            Type type = enumCreator.Compile();
            TypeCreator typeCreator = new TypeCreator("lol2", moduleBuilder);
            typeCreator.AddFields("lol", typeCreator, FieldAttributes.Public);
            Type type1 = typeCreator.Compile();
            return;
        }
    }
}
