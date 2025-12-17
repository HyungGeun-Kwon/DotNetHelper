# DotNetHelper

.NET 프로젝트에서 자주 사용하는 헬퍼/유틸리티 모음입니다.  
주요 모듈은 **MVVMKit**과 **MsDiKit**입니다.

## Modules

### DotNetHelper.MVVMKit
- **Prism 학습 목적**을 포함하며, Prism에서 자주 사용하는 기본 구성 요소를 기준으로 필요한 기능만 간단히 구현했습니다.
- Prism을 사용하지 않더라도 다음을 목표로 합니다.
  - MVVM 구성 요소를 단순하게 유지
  - DI/서비스 등록을 **컨테이너 기반**으로 관리

#### Dialog 내 Region 처리
WPF에서 Dialog(Window)는 Shell의 Visual Tree 밖에서 생성되는 경우가 많아, `RegionName`만 지정할 경우 Region이 연결될 `IRegionManager`가 자동으로 결정되지 않는 상황이 발생할 수 있습니다.  
일반적으로는 Dialog 측에 `RegionManager`(또는 scoped `RegionManager`)를 별도로 연결해야 합니다.

MVVMKit은 이 문제를 단순화하기 위해, 컨테이너에서 `IRegionManager`를 조회하여 Region을 **자동 등록**하도록 구성했습니다.  
따라서 사용자는 Dialog에 `RegionName`만 지정하면 별도의 연결 작업 없이 Region을 사용할 수 있습니다.

> 참고(제약 사항)
> - 전역(컨테이너) `IRegionManager`를 기준으로 등록하므로, 다이얼로그를 다중으로 띄우거나 동일 `RegionName`을 재사용하면 **이름 충돌**이 발생할 수 있습니다.
> - 다이얼로그별로 독립적인 네비게이션/구분이 필요한 경우에는 **scoped RegionManager** 방식이 더 적합할 수 있습니다.

### DotNetHelper.MsDiKit
- Prism에서 사용하던 **Region / Dialog / EventAggregator** 등의 개념을
  `Microsoft.Extensions.DependencyInjection`(MS DI) 기반에서도 **유사한 방식으로 활용**할 수 있도록 구성한 프로젝트입니다.
- Prism의 사용 경험은 일부 유지하면서, 애플리케이션 구성은 **MS DI 중심**으로 가져가는 것을 목표로 합니다.
