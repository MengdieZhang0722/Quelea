﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Collections.ObjectModel;

using OctreeSearch;
using KdTree;

namespace Agent
{
  public class AgentSystemTypeList : GH_Goo<Object>
  {
    private ISpatialCollection<AgentType> agents;
    private AgentType[] agentsSettings;
    private EmitterType[] emitters;
    private ForceType[] forces;
    private BehaviorType[] behaviors;
    private EnvironmentType environment;
    private int timestep;
    private int nextIndex;

    public AgentSystemTypeList()
    {
      this.agents = new SpatialCollectionAsList<AgentType>();
      this.agentsSettings = new AgentType[] { new AgentType() };
      this.emitters = new EmitterType[] { new PtEmitterType() };
      this.environment = null;
      this.forces = new ForceType[] { };
      this.behaviors = new BehaviorType[] { };
      this.timestep = 0;
      this.nextIndex = 0;
    }

    public AgentSystemTypeList(AgentType[] agentsSettings, EmitterType[] emitters, 
                           EnvironmentType environment, ForceType[] forces,
                           BehaviorType[] behaviors)
    {
      this.agents = new SpatialCollectionAsList<AgentType>(); 
      this.agentsSettings = agentsSettings;
      this.emitters = emitters;
      this.environment = environment;
      this.forces = forces;
      this.behaviors = behaviors;
    }

    public AgentSystemTypeList(AgentType[] agentsSettings, EmitterType[] emitters,
                           EnvironmentType environment, ForceType[] forces,
                           BehaviorType[] behaviors, AgentSystemTypeList system)
    {
      this.agents = new SpatialCollectionAsList<AgentType>(system.agents);
      this.agentsSettings = agentsSettings;
      this.emitters = emitters;
      this.environment = environment;
      this.forces = forces;
      this.behaviors = behaviors;
    }

    public AgentSystemTypeList(AgentSystemTypeList system)
    {
      // private ISpatialCollection<AgentType> agents;
      this.agents = new SpatialCollectionAsList<AgentType>(system.agents);
      this.agentsSettings = system.agentsSettings;
      this.emitters = system.emitters;
      this.environment = system.environment;
      this.forces = system.forces;
      this.behaviors = system.behaviors;
    }

    public ISpatialCollection<AgentType> Agents
    {
      get
      {
        return this.agents;
      }
    }

    public IEnumerable<AgentType> AgentsSettings
    {
      get
      {
        return this.agentsSettings;
      }
      set
      {
        this.agentsSettings = (AgentType[]) value;
      }
    }

    public IEnumerable<EmitterType> Emitters
    {
      get
      {
        return this.emitters;
      }
      set // ToDo remove this
      {
        this.emitters = (EmitterType[]) value;
      }
    }

    public IEnumerable<ForceType> Forces
    {
      get
      {
        return this.forces;
      }
      set // ToDo remove this
      {
        this.forces = (ForceType[]) value;
      }
    }

    public IEnumerable<BehaviorType> Behaviors
    {
      get
      {
        return this.behaviors;
      }
      set
      {
        this.behaviors = (BehaviorType[]) value;
      }
    }
    public EnvironmentType Environment
    {
      get
      {
        return this.environment;
      }
      set // ToDo remove this
      {
        this.environment = value;
      }
    }

    public void applyForces(AgentType agent)
    {
      ISpatialCollection<AgentType> neighbors = this.agents.getNeighborsInSphere(agent, agent.VisionRadius);
      foreach (ForceType force in this.forces)
      {
        Vector3d forceVec = force.calcForce(agent, neighbors);
        agent.applyForce(forceVec);
      }
    }

    public bool applyBehaviors(AgentType a)
    {
      bool behaviorsApplied = false;
      foreach (BehaviorType behavior in this.behaviors)
      {
        //if (behavior.applyBehavior(a, this))
        //{
        //  behaviorsApplied = true;
        //}
      }
      return behaviorsApplied;
    }

    public void addAgent(EmitterType emitter)
    {
      Point3d emittionPt = emitter.emit();
      AgentType agent;
      if (environment != null)
      {
        Point3d refEmittionPt = environment.closestRefPoint(emittionPt);
        agent = new AgentType(agentsSettings[nextIndex % agentsSettings.Length], emittionPt, refEmittionPt);
      }
      else
      {
        agent = new AgentType(agentsSettings[nextIndex % agentsSettings.Length], emittionPt);
      }
      agents.Add(agent);
      nextIndex++;
    }

    public void run()
    {
      foreach (EmitterType emitter in emitters)
      {
        if (emitter.ContinuousFlow && (timestep % emitter.CreationRate == 0))
        {
          if ((emitter.NumAgents == 0) || (this.agents.Count < emitter.NumAgents))
          {
            addAgent(emitter);
          }
        }
      }

      IList<AgentType> toRemove = new List<AgentType>();
      foreach (AgentType agent in this.agents) {
        
        if (!applyBehaviors(agent))
        {
          applyForces(agent);
        }
        agent.run();
        if (environment != null)
        {
          agent.RefPosition = environment.closestRefPointOnRef(agent.RefPosition);
          agent.Position = environment.closestPointOnRef(agent.RefPosition);
        }
        else
        {
          agent.RefPosition = agent.Position;
          agent.Position = agent.RefPosition;
        }
        if (agent.isDead())
        {
          toRemove.Add(agent);
        }
      }

      foreach (AgentType deadAgent in toRemove)
      {
        this.agents.Remove(deadAgent);
      }

      timestep++;
    }

    public override bool Equals(object obj) //ToDo fix bugs in equals
    {
      // If parameter is null return false.
      if (obj == null)
      {
        return false;
      }

      // If parameter cannot be cast to Point return false.
      AgentSystemTypeList s = obj as AgentSystemTypeList;
      if ((System.Object)s == null)
      {
        return false;
      }

      // Return true if the fields match:
      return (this.emitters.Equals(s.emitters)) && 
             (this.agentsSettings.Equals(s.agentsSettings)) &&
             (this.environment.Equals(s.environment));
    }

    public bool Equals(AgentSystemTypeList s) //ToDo fix bugs in equals
    {
      // If parameter is null return false:
      if ((object)s == null)
      {
        return false;
      }

      // Return true if the fields match:
      return (this.emitters.Equals(s.emitters)) && 
             (this.agentsSettings.Equals(s.agents)) &&
             (this.environment.Equals(s.environment));
    }

    public override int GetHashCode() //ToDo fix bugs in equals
    {
      int agentHash = 1;
      int emitterHash = 1;
      foreach (AgentType agent in this.agentsSettings)
      {
        agentHash *= agent.GetHashCode();
      }
      foreach (EmitterType emitter in this.emitters)
      {
        emitterHash *= emitter.GetHashCode();
      }
      return agentHash ^ emitterHash ^ this.environment.GetHashCode();
    }

    public override IGH_Goo Duplicate()
    {
      return new AgentSystemTypeList(this);
    }

    public override bool IsValid
    {
      get
      {
        return (this.timestep >= 0) && (this.nextIndex >= 0);
      }
    }

    public override string ToString() //ToDo
    {
      string agents = "Agents: " + this.agentsSettings.Length.ToString() + "\n";
      string emitters = "Emitters: " + this.emitters.Length.ToString() + "\n";
      string environment = "Environment: " + this.environment.ToString() + "\n";
      return agents + emitters + environment;
    }

    public override string TypeDescription
    {
      get { return "An Agent System"; }
    }

    public override string TypeName
    {
      get { return "Agent System"; }
    }
  }
}