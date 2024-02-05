using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class AliasingFollowReferences
    {
        public static void Main(string[] args)
        {
            Contenido c1 = new Contenido();
            c1.Prueba = "valor";
            Contenido c2 = new Contenido();
            c2.Prueba = "algun otro valor";
            List<Contenido> contList = new List<Contenido>();
            contList.Add(c1);
            contList.Add(c2);
            Contenido ultimo = contList.GetAtPosition(1);
            //Console.WriteLine(ultimo.Prueba);
            object referencia = ultimo.Prueba;
        }
        class Contenido
        {
            public object Prueba;
        }
        public class List<T>
        {
            public List()
            {
                root = null;
                Count = 0;
            }

            public class Node
            {
                public Node next = null;
                public T data;
                public Node()
                {
                    next = null;
                }
            }

            // FIXME: no manejamos esto, asi que lo seteamos en constructor
            private Node root = null;

            public T GetAtPosition(int pos)
            {
                // FIXME: Necesitamos manejar los condicionales siempre como statement, ya que si el primer
                // statement de un metodo es IF, saltamos a CONDITION y no hacemos binding
                int dummy = 25;

                if (pos >= Count)
                    return default(T);

                Node curr = root;
                for (int i = 1; i <= pos; i++)
                    curr = curr.next;

                return curr.data;
            }

            // FIXME: Esto era un PropertyAccess y lo cambiamos a una invocacion para que funcione
            public Node Last()
            {
                Node curr = root;
                if (curr == null)
                    return null;
                while (curr.next != null)
                    curr = curr.next;
                return curr;
            }

            // FIXME: no manejamos esto, asi que lo seteamos en constructor
            public int Count = 0;

            // FIXME: Quiza se debe rever el manejo de tipos parametrizados para cuando se debe detectar
            // de consumir o no esta invocacion
            public void Add(T elem)
            {
                Node n = new Node();
                n.data = elem;
                // FIXME: no se puede hacer invocaciones dentro de un condicional! Last() debe estar afuera (o sino entra a Condition)
                Node lastElem = Last();
                if (lastElem == null)
                    root = n;
                else
                    lastElem.next = n;

                Count++;
            }
        }
    }
}
