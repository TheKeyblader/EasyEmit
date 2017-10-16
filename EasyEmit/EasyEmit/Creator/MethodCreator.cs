using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace EasyEmit.Creator
{
    public class MethodCreator : Metadata.MenberData
    {
        private MethodAttributes methodAttributes;

        private CallingConventions callingConventions;

        private Metadata.Metadata returnType;

        private List<Metadata.Metadata> parameters = new List<Metadata.Metadata>();

        private MethodInfo method;
        
        public MethodInfo Method
        {
            get {
                if (State != Metadata.State.NotDefined)
                {
                    return method;
                }
                else
                {
                    return null;
                }
            }
        }

        private MethodBuilder methodBuilder;

        internal MethodCreator(string name,MethodAttributes methodAttributes,CallingConventions callingConventions)
        {
            Name = name;
            State = Metadata.State.NotDefined;
            this.methodAttributes = methodAttributes;
            this.callingConventions = callingConventions;
        }

        public void SetReturnType(Metadata.Metadata returnType)
        {
            if (State != Metadata.State.NotDefined)
            {
                this.returnType = returnType;
            }
            else
            {
                throw new Exception((State == Metadata.State.Creating) ? "The method has been partialy compile" : "The method has been compile");
            }
        } 
        
        public void SetParameters(params Metadata.Metadata[] parameters)
        {
            if (State != Metadata.State.NotDefined)
            {
                if (parameters.Any(m => m == null))
                {
                    throw new ArgumentNullException();
                }
                this.parameters = parameters.ToList();
            }
            else
            {
                throw new Exception((State == Metadata.State.Creating) ? "The method has been partialy compile" : "The method has been compile");
            }
        }

        internal void Compile(TypeBuilder typeBuilder)
        {
            if(State == Metadata.State.Defined)
            {

            }
            MethodBuilder methodBuilder;
            if(State == Metadata.State.NotDefined)
            {
                methodBuilder = CompileDefinition(typeBuilder);
            }
            else
            {
                methodBuilder = this.methodBuilder;
            }
            ILGenerator iLGenerator = methodBuilder.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ret);
            State = Metadata.State.Defined;
        }

        internal MethodBuilder CompileDefinition(TypeBuilder typeBuilder)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(Name, methodAttributes, callingConventions);
            State = Metadata.State.Creating;
            methodBuilder.SetReturnType(returnType);
            methodBuilder.SetParameters((from t in parameters select (Type)t).ToArray());
            this.methodBuilder = methodBuilder;
            return methodBuilder;
        }
    }
}
