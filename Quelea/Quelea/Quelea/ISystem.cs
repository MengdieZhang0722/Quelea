﻿using Grasshopper.Kernel.Types;
using RS = Quelea.Properties.Resources;

namespace Quelea
{
  public interface ISystem : IGH_Goo
  {
    ISpatialCollection<IQuelea> Particles { get; }
    void Run();

    void Populate();

    bool Equals(object obj);

    int GetHashCode();
  }
}
