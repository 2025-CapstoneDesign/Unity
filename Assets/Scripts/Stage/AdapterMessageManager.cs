using System;
using System.Collections.Generic;

/// <summary>
/// 어댑터 패턴을 사용하여 모든 MessageManager를 통합하는 클래스
/// </summary>
public static class AdapterMessageManager
{
    #region 인터페이스 및 어댑터 구현

    /// <summary>
    /// 메시지 어댑터 인터페이스 - 각 상태에 맞는 메시지를 가져오는 공통 인터페이스
    /// </summary>
    public interface IMessageAdapter
    {
        /// <summary>
        /// 주어진 상태에 해당하는 메시지를 가져옵니다.
        /// </summary>
        /// <param name="state">메시지를 가져올 상태 객체</param>
        /// <returns>상태에 맞는 메시지 문자열</returns>
        string GetMessage(object state);
        
        /// <summary>
        /// 이 어댑터가 해당 상태 유형을 처리할 수 있는지 확인합니다.
        /// </summary>
        /// <param name="stateType">확인할 상태의 타입</param>
        /// <returns>처리 가능 여부</returns>
        bool CanHandle(Type stateType);
    }

    /// <summary>
    /// AEDMessageManager에 대한 어댑터 구현
    /// </summary>
    private class CPRMessageAdapter : IMessageAdapter
    {
        public string GetMessage(object state)
        {
            if (state is CPRState cprState)
            {
                return AEDMessageManager.GetMessage(cprState);
            }
            return "알 수 없는 상태입니다.";
        }

        public bool CanHandle(Type stateType)
        {
            return stateType == typeof(CPRState);
        }
    }

    /// <summary>
    /// VacuumSplintMessageManager에 대한 어댑터 구현
    /// </summary>
    private class VacuumSplintMessageAdapter : IMessageAdapter
    {
        public string GetMessage(object state)
        {
            if (state is VacuumSplintState vacuumSplintState)
            {
                return VacuumSplintMessageManager.GetMessage(vacuumSplintState);
            }
            return "알 수 없는 상태입니다.";
        }

        public bool CanHandle(Type stateType)
        {
            return stateType == typeof(VacuumSplintState);
        }
    }

    /// <summary>
    /// TraumaPatientAssessmentMessageManager에 대한 어댑터 구현
    /// </summary>
    private class TraumaPatientAssessmentMessageAdapter : IMessageAdapter
    {
        public string GetMessage(object state)
        {
            if (state is TraumaPatientAssessmentState traumaPatientAssessmentState)
            {
                return TraumaPatientAssessmentMessageManager.GetMessage(traumaPatientAssessmentState);
            }
            return "알 수 없는 상태입니다.";
        }

        public bool CanHandle(Type stateType)
        {
            return stateType == typeof(TraumaPatientAssessmentState);
        }
    }

    /// <summary>
    /// TractionSplintMessageManager에 대한 어댑터 구현
    /// </summary>
    private class TractionSplintMessageAdapter : IMessageAdapter
    {
        public string GetMessage(object state)
        {
            if (state is TractionSplintState tractionSplintState)
            {
                return TractionSplintMessageManager.GetMessage(tractionSplintState);
            }
            return "알 수 없는 상태입니다.";
        }

        public bool CanHandle(Type stateType)
        {
            return stateType == typeof(TractionSplintState);
        }
    }

    /// <summary>
    /// SuctionMessageManager에 대한 어댑터 구현
    /// </summary>
    private class SuctionMessageAdapter : IMessageAdapter
    {
        public string GetMessage(object state)
        {
            if (state is SuctionState suctionState)
            {
                return SuctionMessageManager.GetMessage(suctionState);
            }
            return "알 수 없는 상태입니다.";
        }

        public bool CanHandle(Type stateType)
        {
            return stateType == typeof(SuctionState);
        }
    }

    /// <summary>
    /// SpinalMotionRestrictionMessageManager에 대한 어댑터 구현
    /// </summary>
    private class SpinalMotionRestrictionMessageAdapter : IMessageAdapter
    {
        public string GetMessage(object state)
        {
            if (state is SpinalMotionRestrictionState spinalMotionRestrictionState)
            {
                return SpinalMotionRestrictionMessageManager.GetMessage(spinalMotionRestrictionState);
            }
            return "알 수 없는 상태입니다.";
        }

        public bool CanHandle(Type stateType)
        {
            return stateType == typeof(SpinalMotionRestrictionState);
        }
    }

    /// <summary>
    /// InfantCPRMessageManager에 대한 어댑터 구현
    /// </summary>
    private class InfantCPRMessageAdapter : IMessageAdapter
    {
        public string GetMessage(object state)
        {
            if (state is InfantCPRState infantCPRState)
            {
                return InfantCPRMessageManager.GetMessage(infantCPRState);
            }
            return "알 수 없는 상태입니다.";
        }

        public bool CanHandle(Type stateType)
        {
            return stateType == typeof(InfantCPRState);
        }
    }

    /// <summary>
    /// InfantAirwayMessageManager에 대한 어댑터 구현
    /// </summary>
    private class InfantAirwayMessageAdapter : IMessageAdapter
    {
        public string GetMessage(object state)
        {
            if (state is InfantAirwayState infantAirwayState)
            {
                return InfantAirwayMessageManager.GetMessage(infantAirwayState);
            }
            return "알 수 없는 상태입니다.";
        }

        public bool CanHandle(Type stateType)
        {
            return stateType == typeof(InfantAirwayState);
        }
    }

    #endregion

    #region 어댑터 관리 및 공개 API

    // 사용 가능한 모든 메시지 어댑터들을 저장
    private static readonly List<IMessageAdapter> _adapters = new List<IMessageAdapter>
    {
        new CPRMessageAdapter(),
        new VacuumSplintMessageAdapter(),
        new TraumaPatientAssessmentMessageAdapter(),
        new TractionSplintMessageAdapter(),
        new SuctionMessageAdapter(),
        new SpinalMotionRestrictionMessageAdapter(),
        new InfantCPRMessageAdapter(),
        new InfantAirwayMessageAdapter()
    };

    /// <summary>
    /// 주어진 상태에 대한 메시지를 반환합니다.
    /// </summary>
    /// <param name="state">메시지를 가져올 상태 객체</param>
    /// <returns>상태에 맞는 메시지 문자열</returns>
    public static string GetMessage(object state)
    {
        if (state == null)
        {
            return "상태 객체가 null입니다.";
        }

        Type stateType = state.GetType();
        foreach (var adapter in _adapters)
        {
            if (adapter.CanHandle(stateType))
            {
                return adapter.GetMessage(state);
            }
        }

        return $"처리할 수 있는 어댑터를 찾을 수 없습니다: {stateType.Name}";
    }

    /// <summary>
    /// 특정 타입의 상태에 대한 메시지를 제네릭 메서드로 반환합니다.
    /// </summary>
    /// <typeparam name="T">상태 열거형 타입</typeparam>
    /// <param name="state">상태 열거형 값</param>
    /// <returns>상태에 맞는 메시지 문자열</returns>
    public static string GetMessage<T>(T state) where T : Enum
    {
        return GetMessage((object)state);
    }

    /// <summary>
    /// 새로운 어댑터를 동적으로 추가합니다. 확장을 위한 메서드입니다.
    /// </summary>
    /// <param name="adapter">등록할 어댑터</param>
    public static void RegisterAdapter(IMessageAdapter adapter)
    {
        if (adapter != null && !_adapters.Contains(adapter))
        {
            _adapters.Add(adapter);
        }
    }

    #endregion
}