﻿namespace Quelea
{
  public interface IAgent : IParticle
  {
    double MaxSpeed { get; set; }

    double MaxForce { get; set; }

    double VisionRadius { get; set; }

    double VisionAngle { get; set; }
    double Lon { get; set; }
    double Lat { get; set; }
  }
}