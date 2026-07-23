using System;
using System.Collections.Generic;

namespace OBS.Models;

public partial class Notlar
{
    public int Id { get; set; }

    public int DersKaydiId { get; set; }

    public string OlcmeTuru { get; set; } = null!;

    public decimal Puan { get; set; }

    public virtual DersKaydi DersKaydi { get; set; } = null!;
}
