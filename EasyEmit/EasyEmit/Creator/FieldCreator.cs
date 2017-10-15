using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace EasyEmit.Creator
{
    public class FieldCreator : Metadata.MenberData
    {
        private FieldAttributes fieldAttributes;
        private Metadata.Metadata type;
        internal FieldCreator(string name,Metadata.Metadata metadata,FieldAttributes fieldAttributes)
        {
            this.Name = name;
            this.type = metadata;
            this.State = Metadata.State.NotDefined;
            this.fieldAttributes = fieldAttributes;
        }

        internal void Compile(TypeBuilder typeBuilder)
        {
            if(type.State == Metadata.State.NotDefined)
            {
                throw new Exception(string.Format("The return type {0} of the field {1} has not created or not started created",type.Name,Name));
            }
            FieldBuilder fieldBuilder = typeBuilder.DefineField(Name, type.Type, fieldAttributes);
            State = Metadata.State.Defined;
        }
    }
}