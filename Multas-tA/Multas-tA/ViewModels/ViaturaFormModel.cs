using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Multas_tA.ViewModels
{
    /// <summary>
    /// Um exemplo de um View Model mais útil,
    /// em que colocamos dados para dropdowns no modelo
    /// em vez de usarmos a ViewBag.
    /// </summary>
    public class ViaturaFormModel
    {
        public string Infracao { get; set; }

        public string LocalDaMulta { get; set; }

        // Notar o '?'. Isto permite que o campo receba valores nulos.
        // O '[Required]' só verifica se o campo não é null, e em caso de strings,
        // se a string não está vazia.
        // No entanto, 'int', 'decimal', 'double', etc., têm o problema
        // de não serem nulos, e serem inicializados a 0 em .net quando não têm valor.
        // Isto faria com que o campo aparecesse com um 0, e nunca falharia a validação
        // do obrigatório.
        // No entanto, alguns valores por defeito induzem pessoas em erro (especialmente aquelas
        // que preenchem o formulário à pressa, e depois ficam valores de 0, que até podem
        // ser válidos, mas não é o que as pessoas queriam...
        [Required]
        [Range(0d, double.MaxValue)] // Usamos 'double' porque não existe [Range] para decimal...
        public decimal? ValorMulta { get; set; }

        // Devemos usar [DataType(DataType.Date)] quando queremos datas,
        // [DataType(DataType.DateTime)] quando queremos data e hora.
        // Como não existe suporte para data e hora em todos os browsers,
        // (ver https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/datetime-local)
        // vamos usar Date (só data, sem hora). Para DateTime, usaria um date picker custom (ex: jQuery UI).
        // Existem outros valores úteis que se podem usar no "2º" DataType, como 'EmailAddress'.
        // Mais uma vez, isto pode ser usado nos EditorTemplates (usar o nome, como 'EmailAddress.cshtml').
        [Required, DataType(DataType.Date)]
        public DateTime? DataDaMulta { get; set; }

        public int? IdAgente { get; set; }

        public int? IdViatura { get; set; }

        public int? IdCondutor { get; set; }

        #region Dados para dropdowns.

        public SelectList AgentesSelectList { get; set; }

        public SelectList ViaturasSelectList { get; set; }

        public SelectList CondutoresSelectList { get; set; }

        #endregion
    }
}