using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Multas_tA.Models {
   public class Utilizador {

      public int ID { get; set; }

      public string NomeProprio { get; set; }

      public string Apelido { get; set; }

      public DateTime DataNascimento { get; set; }

      //*********************************************
     /// <summary>
     /// este atributo destina-se a servir como ponte de ligação
     /// entre os objetos desta classe
     /// e os dados da tabela de autenticação
     /// </summary>
      public string UserName { get; set; }
      
   }
}