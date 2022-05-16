using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DeepSpace.Player
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum FleetType
  {
    Mining,
    Fighting,
    Exploration
  }

  public class PlayerFleet
  {
    public string id { get; set; }
    public string type { get; set; }
    public string playerAddr { get; set; }
    public int[] shipIds { get; set; }

    public PlayerFleet()
    {
      this.id = Guid.NewGuid().ToString();
    }
  }
}