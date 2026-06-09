## Description
<!-- Briefly describe the change and why it's needed -->

## Related Issue
<!-- Link to the GitHub issue, if applicable -->

## Type of Change
- [ ] Bug fix
- [ ] New feature (extractor, scenario, detection)
- [ ] Improvement to existing feature
- [ ] Documentation
- [ ] Refactoring / code quality

## Checklist
- [ ] `dotnet build` succeeds with 0 warnings
- [ ] `dotnet test` passes (176+ tests)
- [ ] Golden tests updated if output format changed (`$env:UPDATE_GOLDENS = "1"; dotnet test`)
- [ ] `dotnet format --verify-no-changes` passes
- [ ] XML docs added for new public APIs
- [ ] Changes tested against at least one real project
- [ ] If adding a new extractor: `[JsonDerivedType]` added in `Detections.cs`
- [ ] If adding a new section: registered in relevant scenarios in `ScenarioRegistry.cs`

## Breaking Changes?
- [ ] Yes
- [ ] No

<!-- If yes, describe migration path -->
