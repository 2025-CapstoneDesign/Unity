using System;
using System.Collections.Generic;

public interface IErrorAdapter
{
    string GetLabel(object state);
}

public static class AdapterErrorType
{
    private static readonly Dictionary<Type, IErrorAdapter> _typeAdapters = new Dictionary<Type, IErrorAdapter>
    {
        { typeof(CPRState), new CPRStateAdapter() },
        { typeof(InfantAirwayState), new InfantAirwayStateAdapter() },
        { typeof(InfantCPRState), new InfantCPRStateAdapter() },
        { typeof(SpinalMotionRestrictionState), new SpinalMotionRestrictionStateAdapter() },
        { typeof(SuctionState), new SuctionStateAdapter() },
        { typeof(TractionSplintState), new TractionSplintStateAdapter() },
        { typeof(TraumaPatientAssessmentState), new TraumaPatientAssessmentStateAdapter() },
        { typeof(VacuumSplintState), new VacuumSplintStateAdapter() }
    };

    public static string GetLabel(object state)
    {
        Type stateType = state.GetType();
        
        if (_typeAdapters.TryGetValue(stateType, out IErrorAdapter adapter))
        {
            return adapter.GetLabel(state);
        }
        
        return "PASS";
    }
}

#region Concrete Adapters

public class CPRStateAdapter : IErrorAdapter
{
    public string GetLabel(object state)
    {
        if (state is CPRState cprState)
        {
            return AEDStateToErrorType.GetLabel(cprState);
        }
        return "PASS";
    }
}

public class InfantAirwayStateAdapter : IErrorAdapter
{
    public string GetLabel(object state)
    {
        if (state is InfantAirwayState infantAirwayState)
        {
            return InfantAirwayToErrorType.GetLabel(infantAirwayState);
        }
        return "PASS";
    }
}

public class InfantCPRStateAdapter : IErrorAdapter
{
    public string GetLabel(object state)
    {
        if (state is InfantCPRState infantCPRState)
        {
            return InfantCPRToErrorType.GetLabel(infantCPRState);
        }
        return "PASS";
    }
}

public class SpinalMotionRestrictionStateAdapter : IErrorAdapter
{
    public string GetLabel(object state)
    {
        if (state is SpinalMotionRestrictionState spinalMotionRestrictionState)
        {
            return SpinalMotionRestrictionToErrorType.GetLabel(spinalMotionRestrictionState);
        }
        return "PASS";
    }
}

public class SuctionStateAdapter : IErrorAdapter
{
    public string GetLabel(object state)
    {
        if (state is SuctionState suctionState)
        {
            return SuctionToErrorType.GetLabel(suctionState);
        }
        return "PASS";
    }
}

public class TractionSplintStateAdapter : IErrorAdapter
{
    public string GetLabel(object state)
    {
        if (state is TractionSplintState tractionSplintState)
        {
            return TractionSplintToErrorType.GetLabel(tractionSplintState);
        }
        return "PASS";
    }
}


public class TraumaPatientAssessmentStateAdapter : IErrorAdapter
{
    public string GetLabel(object state)
    {
        if (state is TraumaPatientAssessmentState traumaPatientAssessmentState)
        {
            return TraumaPatientAssessmentToErrorType.GetLabel(traumaPatientAssessmentState);
        }
        return "PASS";
    }
}

public class VacuumSplintStateAdapter : IErrorAdapter
{
    public string GetLabel(object state)
    {
        if (state is VacuumSplintState vacuumSplintState)
        {
            return VacuumSplintToErrorType.GetLabel(vacuumSplintState);
        }
        return "PASS";
    }
}

#endregion