// Copyright (c) 2012 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

namespace UnitTests
{
  using System;
  using System.IO;

  using EnvDTE;
  using EnvDTE80;
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  using NativeClientVSAddIn;

  /// <summary>
  /// This is a test class for PropertyManagerTest and is intended
  /// to contain all PropertyManager Unit Tests
  /// </summary>
  [TestClass]
  public class PropertyManagerTest
  {
    /// <summary>
    /// This holds the path to the NaCl solution used in these tests.
    /// The NaCl solution is a valid NaCl/pepper plug-in VS solution.
    /// It is copied into the testing deployment directory and opened in some tests.
    /// Because unit-tests run in any order, the solution should not be written to
    /// in any tests.
    /// </summary>
    private static string naclSolution;

    /// <summary>
    /// The main visual studio object.
    /// </summary>
    private DTE2 dte_;

    /// <summary>
    /// Gets or sets the test context which provides information about,
    /// and functionality for the current test run.
    /// </summary>
    public TestContext TestContext { get; set; }

    /// <summary>
    /// This is run one time before any test methods are called. Here we set-up a test-copy of a
    /// new NaCl solution for use in the tests.
    /// </summary>
    /// <param name="testContext">Holds information about the current test run</param>
    [ClassInitialize]
    public static void ClassSetup(TestContext testContext)
    {
      DTE2 dte = TestUtilities.StartVisualStudioInstance();
      try
      {
        naclSolution = TestUtilities.CreateBlankValidNaClSolution(
          dte,
          "PropertyManagerTest",
          NativeClientVSAddIn.Strings.PepperPlatformName,
          NativeClientVSAddIn.Strings.NaCl64PlatformName,
          testContext);
      }
      finally
      {
        TestUtilities.CleanUpVisualStudioInstance(dte);
      }
    }

    /// <summary>
    /// This is run before each test to create test resources.
    /// </summary>
    [TestInitialize]
    public void TestSetup()
    {
      dte_ = TestUtilities.StartVisualStudioInstance();
      try
      {
        TestUtilities.AssertAddinLoaded(dte_, NativeClientVSAddIn.Strings.AddInName);
      }
      catch
      {
        TestUtilities.CleanUpVisualStudioInstance(dte_);
        throw;
      }
    }

    /// <summary>
    /// This is run after each test to clean up things created in TestSetup().
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
      TestUtilities.CleanUpVisualStudioInstance(dte_);
    }

    /// <summary>
    /// Tests SetTarget() and SetTargetToActive().
    /// </summary>
    [TestMethod]
    public void SetTargetTest()
    {
      string expectedSDKRootDir =
          Environment.GetEnvironmentVariable(Strings.SDKPathEnvironmentVariable);
      Assert.IsNotNull(expectedSDKRootDir, "SDK Path environment variable not set!");
      expectedSDKRootDir = expectedSDKRootDir.TrimEnd(new char[] { '/', '\\' });

      PropertyManager target = new PropertyManager();
      dte_.Solution.Open(naclSolution);

      Project naclProject = dte_.Solution.Projects.Item(TestUtilities.NaClProjectUniqueName);
      Project notNacl = dte_.Solution.Projects.Item(TestUtilities.NotNaClProjectUniqueName);

      // Invalid project.
      target.SetTarget(notNacl, Strings.PepperPlatformName, "Debug");
      Assert.AreEqual(
          PropertyManager.ProjectPlatformType.Other,
          target.PlatformType,
          "SetTarget should not succeed with non-nacl/pepper project.");

      // Try valid project with different platforms.
      target.SetTarget(naclProject, Strings.NaCl64PlatformName, "Debug");
      Assert.AreEqual(
          PropertyManager.ProjectPlatformType.NaCl,
          target.PlatformType,
          "SetTarget did not succeed with NaCl64 platform on valid project.");
      Assert.AreEqual(expectedSDKRootDir, target.SDKRootDirectory, "SDK Root incorrect.");

      target.SetTarget(naclProject, Strings.NaClARMPlatformName, "Debug");
      Assert.AreEqual(
          PropertyManager.ProjectPlatformType.NaCl,
          target.PlatformType,
          "SetTarget did not succeed with NaClARM platform on valid project.");
      Assert.AreEqual(expectedSDKRootDir, target.SDKRootDirectory, "SDK Root incorrect.");

      target.SetTarget(naclProject, Strings.PNaClPlatformName, "Debug");
      Assert.AreEqual(
          PropertyManager.ProjectPlatformType.NaCl,
          target.PlatformType,
          "SetTarget did not succeed with nacl platform on valid project.");
      Assert.AreEqual(expectedSDKRootDir, target.SDKRootDirectory, "SDK Root incorrect.");

      target.SetTarget(naclProject, Strings.PepperPlatformName, "Debug");
      Assert.AreEqual(
          PropertyManager.ProjectPlatformType.Pepper,
          target.PlatformType,
          "SetTarget did not succeed with pepper platform on valid project.");
      Assert.AreEqual(expectedSDKRootDir, target.SDKRootDirectory, "SDK Root incorrect.");

      target.SetTarget(naclProject, "Win32", "Debug");
      Assert.AreEqual(
          PropertyManager.ProjectPlatformType.Other,
          target.PlatformType,
          "SetTarget did not set 'other' platform on when Win32 platform of valid project.");

      // Setting the start-up project to a non-cpp project should make loading fail.
      object[] badStartupProj = { TestUtilities.NotNaClProjectUniqueName };
      dte_.Solution.SolutionBuild.StartupProjects = badStartupProj;
      target.SetTargetToActive(dte_);
      Assert.AreEqual(
          PropertyManager.ProjectPlatformType.Other,
          target.PlatformType,
          "SetTargetToActive should not succeed with non-nacl/pepper project.");

      // Setting the start-up project to correct C++ project, but also setting the platform
      // to non-nacl/pepper should make loading fail.
      object[] startupProj = { TestUtilities.NaClProjectUniqueName };
      dte_.Solution.SolutionBuild.StartupProjects = startupProj;
      TestUtilities.SetSolutionConfiguration(dte_, TestUtilities.NaClProjectUniqueName,
                                             "Debug", "Win32");
      target.SetTargetToActive(dte_);
      Assert.AreEqual(
          PropertyManager.ProjectPlatformType.Other,
          target.PlatformType,
          "SetTargetToActive should not succeed with Win32 platform.");

      // Now setting the platform to NaCl64 should make this succeed.
      TestUtilities.SetSolutionConfiguration(dte_, TestUtilities.NaClProjectUniqueName,
                                             "Debug", Strings.NaCl64PlatformName);
      target.SetTargetToActive(dte_);
      Assert.AreEqual(
          PropertyManager.ProjectPlatformType.NaCl,
          target.PlatformType,
          "SetTargetToActive should succeed with NaCl platform and valid project.");
      Assert.AreEqual(expectedSDKRootDir, target.SDKRootDirectory, "SDK Root incorrect.");
    }

    /// <summary>
    /// A test for GetProperty. Checks some non-trivial C# properties and the GetProperty method.
    /// </summary>
    [TestMethod]
    public void GetPropertyTest()
    {
      string expectedSDKRootDir =
          Environment.GetEnvironmentVariable(Strings.SDKPathEnvironmentVariable);
      Assert.IsNotNull(expectedSDKRootDir, "SDK Path environment variable not set!");
      expectedSDKRootDir = expectedSDKRootDir.TrimEnd(new char[] { '/', '\\' });

      // Set up the property manager to read the NaCl platform settings from BlankValidSolution.
      PropertyManager target = new PropertyManager();
      dte_.Solution.Open(naclSolution);
      Project naclProject = dte_.Solution.Projects.Item(TestUtilities.NaClProjectUniqueName);

      string slnDir = Path.GetDirectoryName(naclSolution);
      string projectDir = Path.Combine(
          slnDir, Path.GetDirectoryName(TestUtilities.NaClProjectUniqueName)) + @"\";

      foreach (string platform in NaClPlatformNames())
      {
          target.SetTarget(naclProject, platform, "Debug");
          Assert.AreEqual(
              PropertyManager.ProjectPlatformType.NaCl,
              target.PlatformType,
              "SetTarget did not succeed with nacl platform on valid project.");

          string outputDir = Path.Combine(projectDir, platform, "newlib", "Debug") + @"\";

          string assembly = Path.Combine(outputDir, TestUtilities.BlankNaClProjectName);
          if (platform == "NaCl64")
              assembly += "_64.nexe";
          else if (platform == "NaCl32")
              assembly += "_32.nexe";
          else if (platform == "NaClARM")
              assembly += "_arm.nexe";
          else if (platform == "PNaCl")
              assembly += ".pexe";
          else
              Assert.Fail();

          Assert.AreEqual(projectDir, target.ProjectDirectory, "ProjectDirectory.");
          Assert.AreEqual(outputDir, target.OutputDirectory, "OutputDirectory.");
          Assert.AreEqual(assembly, target.PluginAssembly, "PluginAssembly.");
          Assert.AreEqual(expectedSDKRootDir, target.SDKRootDirectory, "SDK Root.");
          Assert.AreEqual(
              @"newlib",
              target.GetProperty("ConfigurationGeneral", "ToolchainName"),
              "GetProperty() with ToolchainName incorrect.");
      }
    }

    /// <summary>
    /// Return a list of all nacl platform names.
    /// </summary>
    public static string[] NaClPlatformNames()
    {
        return new string[] { Strings.NaCl32PlatformName, Strings.NaCl64PlatformName,
                              Strings.NaClARMPlatformName, Strings.PNaClPlatformName };
    }

    /// <summary>
    /// A test for SetProperty.
    /// </summary>
    [TestMethod]
    public void SetPropertyTest()
    {
      string setTargetSolution = TestUtilities.CreateBlankValidNaClSolution(
          dte_,
          "PropertyManagerTestSetTarget",
          NativeClientVSAddIn.Strings.NaCl64PlatformName,
          NativeClientVSAddIn.Strings.NaCl64PlatformName,
          TestContext);

      // Set up the property manager to read the NaCl platform settings from BlankValidSolution.
      PropertyManager target = new PropertyManager();
      dte_.Solution.Open(setTargetSolution);
      Project naclProject = dte_.Solution.Projects.Item(TestUtilities.NaClProjectUniqueName);

      foreach (string platform in NaClPlatformNames())
      {
          target.SetTarget(naclProject, platform, "Debug");
          Assert.AreEqual(
              PropertyManager.ProjectPlatformType.NaCl,
              target.PlatformType,
              "SetTarget did not succeed with nacl platform on valid project.");

          string newValue = "ThisIsNew";
          target.SetProperty("ConfigurationGeneral", "VSNaClSDKRoot", newValue);
          Assert.AreEqual(
              newValue,
              target.GetProperty("ConfigurationGeneral", "VSNaClSDKRoot"),
              "SetProperty() did not set property VSNaClSDKRoot.");
      }
    }
  }
}
