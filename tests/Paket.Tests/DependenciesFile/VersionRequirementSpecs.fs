module Paket.DependenciesFile.VersionRequirementSpecs

open Paket
open NUnit.Framework
open FsUnit
open Paket.Requirements
open Paket.Domain

let parse text = DependenciesFileParser.parseVersionRequirement(text)

let require packageName strategyForTransitives text : PackageRequirement = 
    { Name = PackageName packageName
      VersionRequirement = parse text
      ResolverStrategyForDirectDependencies = None
      ResolverStrategyForTransitives = Some strategyForTransitives
      Parent = PackageRequirementSource.DependenciesFile("",0)
      Graph = Set.empty
      Sources = []
      Kind = PackageRequirementKind.Package
      TransitivePrereleases = false
      Settings = InstallSettings.Default }

[<Test>]
let ``can order simple at least requirements``() = 
    parse ">= 2.2" |> shouldBeSmallerThan (parse ">= 2.3")
    parse ">= 1.2" |> shouldBeSmallerThan (parse ">= 2.3")

[<Test>]
let ``can order twiddle-wakka``() = 
    parse "~> 2.2" |> shouldBeSmallerThan (parse "~> 2.3")
    parse "~> 1.2" |> shouldBeSmallerThan (parse "~> 2.3")

[<Test>]
let ``can parse twiddle-wakka with prerelease``() = 
    let req = DependenciesFileParser.parseVersionRequirement "~> 6.0.0 prerelease"
    
    let included = SemVer.Parse("6.0.0")
    let excluded = SemVer.Parse("6.1.0")

    req
    |> shouldEqual 
        (VersionRequirement.VersionRequirement(
            VersionRange.Range(VersionRangeBound.Including,included,excluded,VersionRangeBound.Excluding),
            PreReleaseStatus.All))

    req.IsInRange(included) |> Assert.IsTrue
    req.IsInRange(excluded) |> Assert.IsFalse
    req.IsInRange(SemVer.Parse "6.0.9-beta.8") |> Assert.IsTrue
    
    req.FormatInNuGetSyntax() |> shouldEqual "[6.0.0-prerelease,6.1.0)"
    
[<Test>]
let ``can parse twiddle-wakka with embedded prerelease``() = 
    let req = DependenciesFileParser.parseVersionRequirement "~> 6.0.0-beta"
    
    let included = SemVer.Parse("6.0.0-beta") 
    let excluded = SemVer.Parse("6.1.0")

    req
    |> shouldEqual 
        (VersionRequirement.VersionRequirement(
            VersionRange.Range(VersionRangeBound.Including,included,excluded,VersionRangeBound.Excluding),
            PreReleaseStatus.Concrete ["beta"]))

    req.IsInRange(included) |> Assert.IsTrue
    req.IsInRange(excluded) |> Assert.IsFalse
    req.IsInRange(SemVer.Parse "6.0.9-beta.8") |> Assert.IsTrue
    
    req.FormatInNuGetSyntax() |> shouldEqual "[6.0.0-beta,6.1.0)"
    
[<Test>]
let ``can parse twiddle-wakka with embedded semver2 prerelease``() = 
    let req = DependenciesFileParser.parseVersionRequirement "~> 6.0.0-beta.1"
    
    let included = SemVer.Parse("6.0.0-beta.1")
    let excluded = SemVer.Parse("6.1.0")

    req
    |> shouldEqual 
        (VersionRequirement.VersionRequirement(
            VersionRange.Range(VersionRangeBound.Including,included,excluded,VersionRangeBound.Excluding),
            PreReleaseStatus.Concrete ["beta"]))

    req.IsInRange(included) |> Assert.IsTrue
    req.IsInRange(excluded) |> Assert.IsFalse
    req.IsInRange(SemVer.Parse "6.0.9-beta.8") |> Assert.IsTrue
    
    req.FormatInNuGetSyntax() |> shouldEqual "[6.0.0-beta.1,6.1.0)"

[<Test>]
let ``can parse twiddle-wakka with specific prerelease``() = 
    let req = DependenciesFileParser.parseVersionRequirement "~> 6.0.0 beta"
    
    let included = SemVer.Parse("6.0.0")
    let excluded = SemVer.Parse("6.1.0")

    req
    |> shouldEqual 
        (VersionRequirement.VersionRequirement(
            VersionRange.Range(VersionRangeBound.Including,included,excluded,VersionRangeBound.Excluding),
            PreReleaseStatus.Concrete ["beta"]))

    req.IsInRange(included) |> Assert.IsTrue
    req.IsInRange(excluded) |> Assert.IsFalse
    req.IsInRange(SemVer.Parse "6.0.9-beta.8") |> Assert.IsTrue
    
    req.FormatInNuGetSyntax() |> shouldEqual "[6.0.0-beta,6.1.0)"
    
[<Test>]
let ``can parse twiddle-wakka with specific embedded prerelease``() = 
    let req = DependenciesFileParser.parseVersionRequirement "~> 6.0.0-beta"
    
    let included = SemVer.Parse("6.0.0-beta") 
    let excluded = SemVer.Parse("6.1.0")

    req
    |> shouldEqual 
        (VersionRequirement.VersionRequirement(
            VersionRange.Range(VersionRangeBound.Including,included,excluded,VersionRangeBound.Excluding),
            PreReleaseStatus.Concrete ["beta"]))

    req.IsInRange(included) |> Assert.IsTrue
    req.IsInRange(excluded) |> Assert.IsFalse
    req.IsInRange(SemVer.Parse "6.0.9-beta.8") |> Assert.IsTrue
    
    req.FormatInNuGetSyntax() |> shouldEqual "[6.0.0-beta,6.1.0)"
    
[<Test>]
let ``can parse twiddle-wakka with specific embedded semver2 prerelease``() = 
    let req = DependenciesFileParser.parseVersionRequirement "~> 6.0.0-beta.7" 

    let included = SemVer.Parse "6.0.0-beta.7"
    let excluded = SemVer.Parse "6.1.0"

    req
    |> shouldEqual 
        (VersionRequirement.VersionRequirement(
            VersionRange.Range(VersionRangeBound.Including,included,excluded,VersionRangeBound.Excluding),
            PreReleaseStatus.Concrete ["beta"]))

    req.IsInRange(included) |> Assert.IsTrue
    req.IsInRange(excluded) |> Assert.IsFalse
    req.IsInRange(SemVer.Parse "6.0.9-beta.8") |> Assert.IsTrue
    
    req.FormatInNuGetSyntax() |> shouldEqual "[6.0.0-beta.7,6.1.0)"

[<Test>]
let ``can parse doubled prerelease``() = 
    let req = DependenciesFileParser.parseVersionRequirement "0.33.0-beta prerelease" 
    
    let included = SemVer.Parse("0.33.0-beta")

    req
    |> shouldEqual 
        (VersionRequirement.VersionRequirement(
            VersionRange.Specific(included),
            PreReleaseStatus.All))

    req.FormatInNuGetSyntax()
    |> shouldEqual "[0.33.0-beta]"
    
    req.IsInRange(included) |> Assert.IsTrue
    
[<Test>]
let ``can parse doubled specific prerelease``() = 
    let req = DependenciesFileParser.parseVersionRequirement "0.33.0-beta.1 beta" 

    let included = SemVer.Parse("0.33.0-beta.1")
    
    req
    |> shouldEqual 
        (VersionRequirement.VersionRequirement(
            VersionRange.Specific(included),
            PreReleaseStatus.Concrete ["beta"]))

    req.FormatInNuGetSyntax()
    |> shouldEqual "[0.33.0-beta.1]"
    
    req.IsInRange(included) |> Assert.IsTrue
    
[<Test>]
let ``can parse twiddle-wakka with specific embedded and doubled semver2 prerelease``() = 
    let req = DependenciesFileParser.parseVersionRequirement "~> 6.0.0-beta.7 beta" 

    let included = SemVer.Parse "6.0.0-beta.7"
    let excluded = SemVer.Parse "6.1.0"

    req
    |> shouldEqual 
        (VersionRequirement.VersionRequirement(
            VersionRange.Range(VersionRangeBound.Including,included,excluded,VersionRangeBound.Excluding),
            PreReleaseStatus.Concrete ["beta"]))

    req.IsInRange(included) |> Assert.IsTrue
    req.IsInRange(excluded) |> Assert.IsFalse
    req.IsInRange(SemVer.Parse "6.0.9-beta.8") |> Assert.IsTrue
    
    req.FormatInNuGetSyntax() |> shouldEqual "[6.0.0-beta.7,6.1.0)"

[<Test>]
let ``can order simple at least requirements in package requirement``() = 
    require "A" ResolverStrategy.Max ">= 2.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Max ">= 2.3")
    require "A" ResolverStrategy.Max ">= 1.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Max ">= 2.3")
    
    require "A" ResolverStrategy.Min ">= 2.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Min ">= 2.3")
    require "A" ResolverStrategy.Min ">= 1.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Min ">= 2.3")

[<Test>]
let ``can order alphabetical if everything else is equal``() = 
    require "B" ResolverStrategy.Max ">= 2.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Max ">= 2.2")
    require "B" ResolverStrategy.Max ">= 1.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Max ">= 1.2")
    
    require "C" ResolverStrategy.Min ">= 2.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Min ">= 2.2")
    require "C" ResolverStrategy.Min ">= 1.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Min ">= 1.2")

[<Test>]
let ``can order twiddle-wakka in package requirement``() = 
    require "A" ResolverStrategy.Max "~> 2.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Max "~> 2.3")
    require "A" ResolverStrategy.Max "~> 1.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Max "~> 2.3")
    
    require "A" ResolverStrategy.Min "~> 2.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Min "~> 2.3")
    require "A" ResolverStrategy.Min "~> 1.2" |> shouldBeGreaterThan (require "A" ResolverStrategy.Min "~> 2.3")

[<Test>]
let ``can order resolver strategy``() = 
    require "A" ResolverStrategy.Min "~> 2.2" |> shouldBeSmallerThan (require "A" ResolverStrategy.Max "~> 2.3")
    require "A" ResolverStrategy.Min "~> 2.2" |> shouldBeSmallerThan (require "A" ResolverStrategy.Max "~> 2.1")
    require "A" ResolverStrategy.Min "~> 2.4" |> shouldBeSmallerThan (require "A" ResolverStrategy.Max "~> 1.3")

[<Test>]
let ``can order naming and range``() = 
    require "Castle.Windsor-NLog" ResolverStrategy.Min ">= 0" |> shouldBeGreaterThan (require "Nancy.Bootstrappers.Windsor" ResolverStrategy.Min "~> 0.23")