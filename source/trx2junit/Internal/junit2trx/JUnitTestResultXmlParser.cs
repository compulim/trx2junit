﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using trx2junit.Models;

namespace trx2junit
{
    public class JUnitTestResultXmlParser : ITestResultXmlParser<JUnitTest>
    {
        private readonly XElement  _junit;
        private readonly JUnitTest _test = new JUnitTest();
        //---------------------------------------------------------------------
        public JUnitTestResultXmlParser(XElement? junit) => _junit = junit ?? throw new ArgumentNullException(nameof(junit));
        //---------------------------------------------------------------------
        public JUnitTest Result => _test;
        //---------------------------------------------------------------------
        public void Parse()
        {
            foreach (XElement xTestSuite in _junit.Elements())
            {
                JUnitTestSuite testSuite = ParseTestSuite(xTestSuite);
                _test.TestSuites.Add(testSuite);
            }
        }
        //---------------------------------------------------------------------
        private static JUnitTestSuite ParseTestSuite(XElement xTestSuite)
        {
            var testSuite = new JUnitTestSuite
            {
                Name          = xTestSuite.Attribute("name").Value,
                HostName      = xTestSuite.Attribute("hostname").Value,
                Id            = xTestSuite.ReadInt("id"),
                TestCount     = xTestSuite.ReadInt("tests"),
                ErrorCount    = xTestSuite.ReadInt("errors"),
                FailureCount  = xTestSuite.ReadInt("failures"),
                SkippedCount  = xTestSuite.ReadInt("skipped"),
                TimeInSeconds = xTestSuite.ReadDouble("time"),
                TimeStamp     = xTestSuite.ReadDateTime("timestamp")!.Value,
            };

            XElement? xStdErr = xTestSuite.Element("system-err");
            if (xStdErr != null)
            {
                testSuite.SystemErr = xStdErr.Value;
            }

            XElement? xStdOut = xTestSuite.Element("system-out");
            if (xStdOut != null)
            {
                testSuite.SystemOut = xStdOut.Value;
            }

            foreach(XElement xProperty in xTestSuite.Elements("properties"))
            {
                if (TryParseProperty(xProperty, out JUnitProperty? property))
                {
                    testSuite.Properties.Add(property);
                }
            }

            foreach (XElement xTestCase in xTestSuite.Elements("testcase"))
            {
                JUnitTestCase testCase = ParseTestCase(xTestCase);
                testSuite.TestCases.Add(testCase);
            }

            return testSuite;
        }
        //---------------------------------------------------------------------
        private static bool TryParseProperty(XElement xProperty, [NotNullWhen(true)] out JUnitProperty? property)
        {
            if (!xProperty.HasAttributes)
            {
                property = null;
                return false;
            }

            property = new JUnitProperty
            {
                Name  = xProperty.Attribute("name").Value,
                Value = xProperty.Attribute("value").Value
            };

            return true;
        }
        //---------------------------------------------------------------------
        private static JUnitTestCase ParseTestCase(XElement xTestCase)
        {
            var testCase = new JUnitTestCase
            {
                Name          = xTestCase.Attribute("name").Value,
                ClassName     = xTestCase.Attribute("classname").Value,
                TimeInSeconds = xTestCase.ReadDouble("time"),
            };

            XElement? xSkipped = xTestCase.Element("skipped");
            if (xSkipped != null)
            {
                testCase.Skipped = true;
            }

            XElement? xFailure = xTestCase.Element("failure");
            if (xFailure != null)
            {
                testCase.Error = new JUnitError
                {
                    Type    = xFailure.Attribute("type").Value,
                    Message = xFailure.Attribute("message").Value
                };
            }

            return testCase;
        }
    }
}