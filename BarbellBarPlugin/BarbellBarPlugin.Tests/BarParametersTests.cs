using System.Linq;
using BarbellBarPlugin.Model;
using BarbellBarPlugin.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BarbellBarPlugin.Tests
{
    //TODO: XML
    [TestClass]
    public class BarParametersTests
    {
        //TODO: description
        [TestMethod]
        public void Constructor_AssignsProperties()
        {
            double sleeveDiameter = 30;
            double separatorLength = 50;
            double handleLength = 1200;
            double separatorDiameter = 40;
            double sleeveLength = 350;

            //TODO: RSDN
            var p = new BarParameters(
                sleeveDiameter,
                separatorLength,
                handleLength,
                separatorDiameter,
                sleeveLength);

            Assert.AreEqual(sleeveDiameter, p.SleeveDiameter);
            Assert.AreEqual(separatorLength, p.SeparatorLength);
            Assert.AreEqual(handleLength, p.HandleLength);
            Assert.AreEqual(separatorDiameter, p.SeparatorDiameter);
            Assert.AreEqual(sleeveLength, p.SleeveLength);
        }

        //TODO: description
        [TestMethod]
        public void TotalLength_CalculatedCorrectly()
        {
            //TODO: RSDN
            var p = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            double total = p.TotalLength;

            Assert.AreEqual(2000.0, total, 1e-6);
        }

        //TODO: description
        [TestMethod]
        public void Validator_ValidParameters_ReturnsNoErrors()
        {
            //TODO: RSDN
            var p = new BarParameters(
                sleeveDiameter: 30,  
                separatorLength: 50,  
                handleLength: 1250,   
                separatorDiameter: 40,
                sleeveLength: 350     
            );

            var errors = BarParametersValidator.Validate(p);

            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void Validator_SleeveDiameterOutOfRange_AddsError()
        {
            var p = new BarParameters(
                sleeveDiameter: 10,  
                separatorLength: 50,
                handleLength: 1250,
                separatorDiameter: 40,
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(p);

            Assert.IsTrue(errors.Any(e => e.FieldName == "DiametrSleeve"));
        }

        [TestMethod]
        public void Validator_SeparatorLengthOutOfRange_AddsError()
        {
            var p = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 10,  
                handleLength: 1250,
                separatorDiameter: 40,
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(p);

            Assert.IsTrue(errors.Any(e => e.FieldName == "LengthSeparator"));
        }

        [TestMethod]
        public void Validator_HandleLengthOutOfRange_AddsError()
        {
            var p = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 200,    
                separatorDiameter: 40,
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(p);

            Assert.IsTrue(errors.Any(e => e.FieldName == "LengthHandle"));
        }

        [TestMethod]
        public void Validator_SeparatorDiameterOutOfRange_AddsError()
        {
            var p = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1250,
                separatorDiameter: 10, 
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(p);

            Assert.IsTrue(errors.Any(e => e.FieldName == "DiametrSeparator"));
        }

        [TestMethod]
        public void Validator_SleeveLengthOutOfRange_AddsError()
        {
            var p = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1250,
                separatorDiameter: 40,
                sleeveLength: 100     
            );

            var errors = BarParametersValidator.Validate(p);

            Assert.IsTrue(errors.Any(e => e.FieldName == "LengthSleeve"));
        }

        [TestMethod]
        public void Validator_SeparatorNotGreaterThanSleeve_AddsError()
        {
            var p = new BarParameters(
                sleeveDiameter: 35,
                separatorLength: 50,
                handleLength: 1250,
                separatorDiameter: 35, 
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(p);

            Assert.IsTrue(errors.Any(e => e.FieldName == "DiametrSeparator"));
        }

        [TestMethod]
        public void Validator_HandleTooShortComparedToSeparators_AddsError()
        {
            var p = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 80,  
                separatorDiameter: 40,
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(p);

            Assert.IsTrue(errors.Any(e => e.FieldName == "LengthHandle"));
        }
    }
}
