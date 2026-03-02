using ProductionPlanning.Models;
using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProductionPlanning.Extensions
{
    public static class NoteExtensions
    {
        public static string GetNumString(this Note _note)
        {
            return _note.Number.ToString("D4");
        }

        public static string GetNumString(this Note _note, string _prefix)
        {
            return $"{_prefix}{_note.Number.ToString("D4")}";
        }
    }
}
