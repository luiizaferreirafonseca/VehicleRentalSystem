using System;
using System.Collections.Generic;
using NUnit.Framework;
using VehicleRentalSystem.DTO;

namespace VehicleSystem.Tests.DTOs
{
    [TestFixture]
    [Category("DTOs")]
    public class VehicleListResponseDTOTests
    {
        // ─── VehicleListResponseDTO ───────────────────────────────────────────────

        #region VehicleListResponseDTO

        [Test]
        [Category("Unit")]
        public void Default_VehiclesNaoEhNulo()
        {
            var dto = new VehicleListResponseDTO();

            Assert.That(dto.Vehicles, Is.Not.Null);
        }

        [Test]
        [Category("Unit")]
        public void Default_VehiclesEhListaVazia()
        {
            var dto = new VehicleListResponseDTO();

            Assert.That(dto.Vehicles, Is.Empty);
        }

        [Test]
        [Category("Unit")]
        public void Default_MessageEhNula()
        {
            var dto = new VehicleListResponseDTO();

            Assert.That(dto.Message, Is.Null);
        }

        [Test]
        [Category("Unit")]
        public void ComVeiculos_VehiclesContemItensAdicionados()
        {
            var dto = new VehicleListResponseDTO
            {
                Vehicles = new List<VehicleResponseDTO>
                {
                    new() { Id = Guid.NewGuid(), Brand = "Honda",  Model = "Civic",   Status = "available" },
                    new() { Id = Guid.NewGuid(), Brand = "Toyota", Model = "Corolla", Status = "available" }
                }
            };

            Assert.That(dto.Vehicles, Has.Count.EqualTo(2));
        }

        [Test]
        [Category("Unit")]
        public void ComMensagem_MessageEhDefinidaCorretamente()
        {
            const string expected = "Não há veículos disponíveis para locação.";
            var dto = new VehicleListResponseDTO { Message = expected };

            Assert.That(dto.Message, Is.EqualTo(expected));
        }

        #endregion

        // ─── VehicleResponseDTO ───────────────────────────────────────────────────

        #region VehicleResponseDTO

        [Test]
        [Category("Unit")]
        public void VehicleResponseDTO_PropriedadesMapeadasCorretamente()
        {
            var id = Guid.NewGuid();
            var dto = new VehicleResponseDTO
            {
                Id = id,
                Brand = "Ford",
                Model = "Ka",
                Year = 2021,
                DailyRate = 120m,
                Status = "available",
                LicensePlate = "DEF-5678"
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.EqualTo(id));
                Assert.That(dto.Brand, Is.EqualTo("Ford"));
                Assert.That(dto.Model, Is.EqualTo("Ka"));
                Assert.That(dto.Year, Is.EqualTo(2021));
                Assert.That(dto.DailyRate, Is.EqualTo(120m));
                Assert.That(dto.Status, Is.EqualTo("available"));
                Assert.That(dto.LicensePlate, Is.EqualTo("DEF-5678"));
            });
        }

        #endregion
    }
}
