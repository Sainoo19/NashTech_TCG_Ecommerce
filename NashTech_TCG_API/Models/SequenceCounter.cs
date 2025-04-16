using System.ComponentModel.DataAnnotations;

namespace NashTech_TCG_API.Models
{
    public class SequenceCounter
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        public int Sequence { get; set; } = 0;
    }
}
