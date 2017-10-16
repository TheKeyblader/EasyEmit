using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace EasyEmit.Creator
{
    public class GenerericParameterCreator:Metadata.Metadata
    {

        private Metadata.Metadata typeConstraint;
        private List<Metadata.Metadata> interfaces = new List<Metadata.Metadata>();

        internal GenerericParameterCreator(string name,Assembly assembly)
        {
            Name = name;
            State = Metadata.State.NotDefined;
            IsClass = false;
            IsEnum = false;
            IsInterface = false;
            IsGenericParameter = true;
            Assembly = assembly;
            
        }
        
        public void SetBaseConstraint(Metadata.Metadata type)
        {
            if(!type.IsClass)
            {
                throw new Exception("The type is not and class");
            }
            this.typeConstraint = type;
        }

        public void SetInterfacesConstraint(params Metadata.Metadata[] interfaces)
        {
            if(interfaces.Any(i => !i.IsInterface))
            {
                throw new Exception("");
            }
            this.interfaces = interfaces.ToList();
        }


        internal void Compile(GenericTypeParameterBuilder genericTypeParameterBuilder)
        {
            
        }
    }
}
