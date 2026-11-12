# OmniSight.UI

OmniSight 계열 프로젝트가 공용으로 사용하는 WinForms 커스텀 UI 컨트롤 라이브러리 (출력: `OmniSight.UI.dll`).

## 구조
- `src/OmniSight.UI` : 라이브러리 (net9.0-windows, net472 멀티타깃)
- `samples/OmniSight.UI.Demo` : 컨트롤 확인용 데모 앱

## 컨트롤
- `ThemedButton` : Theme 색을 쓰는 플랫 버튼(파이프라인 검증용 샘플)
- `DeviceStatusStrip` : 장치 연결 상태를 램프 + 이름 칩으로 보여 주는 헤더용 표시줄. 6단계 상태, 다크·라이트 테마, 상세 팝업. 사용법·크기 조절·결과 화면은 [docs/DeviceStatusStrip.md](docs/DeviceStatusStrip.md)

## 빌드
```
dotnet build OmniSight.UI.sln -c Release
```
DLL 위치: `src/OmniSight.UI/bin/Release/<TFM>/OmniSight.UI.dll`

## 프로젝트에서 참조
버전별 폴더에 DLL을 배포하고 HintPath로 참조한다.
```xml
<ItemGroup>
  <Reference Include="OmniSight.UI">
    <HintPath>경로\OmniSight.UI\0.1.0\net9.0-windows\OmniSight.UI.dll</HintPath>
  </Reference>
</ItemGroup>
```

## 버전 규칙
- SemVer, 버전은 `OmniSight.UI.csproj`의 `<Version>` 하나로 관리
- 릴리스 시 Git 태그 `vX.Y.Z` 생성, 호환성 깨지면 메이저 증가
- 컨트롤 수정은 이 저장소에서만 한다 (소비 프로젝트에 UI 소스 복사 금지)
