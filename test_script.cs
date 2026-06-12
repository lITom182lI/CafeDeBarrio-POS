using System;
using System.Collections.Generic;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;
using CafeBarrio.Application.Common.Helpers;
using MUIS_CORE.Wrappers;

public class TestRunner
{
    public static void Main()
    {
        decimal subtotal = 10m;
        decimal tasaIgv = 0.18m;
        decimal? montoMetodoPrimario = 5.00m;
        
        var impuestoEstimado = MoneyRounding.Round(subtotal * tasaIgv);
        var totalEstimado = subtotal + impuestoEstimado;
        
        Console.WriteLine($"Total estimado: {totalEstimado}");
        
        if (montoMetodoPrimario.Value >= totalEstimado)
        {
            Console.WriteLine("Failure: Pago.MontoMetodoPrimarioExcedido");
        }
        else
        {
            Console.WriteLine("Success: Pass validation");
        }
    }
}
