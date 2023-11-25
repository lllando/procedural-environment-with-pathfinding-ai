using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class CalculateDistributions
{
    
    public static int ExponentialDistribution(double lambda, int minValue = 0)
    {
        double randomValue = Random.value; // Used to generate a random value from 0 to 1 (uniform distribution)
        double exponentialValue = -Math.Log(randomValue) / lambda; // Skews the random value using an exponential distribution

        return Math.Max(minValue, (int)Math.Round(exponentialValue)); // Set a minimum damage of 1 and return the rounded exponential value if it is greater than 1
    }
    
    public static int NormalDistribution(double meanValue, double standardDeviation)
    {
        double uniformRandom1 = 1.0 - Random.value; // Get a random value between 0 and 1
        double uniformRandom2 = 1.0 - Random.value; // Get a random value between 0 and 1

        // Uses Box-Muller transform algorithm to pass in two random values and output a normally distributed one
        double standardNormalRandom = Math.Sqrt(-2.0 * Math.Log(uniformRandom1)) * Math.Sin(2.0 * Math.PI * uniformRandom2);

        double normalDamageValue = meanValue + standardDeviation * standardNormalRandom; // Adjust the normal distribution value based on mean and standard deviation

        return Math.Max(1, (int)Math.Round(normalDamageValue)); // Ensure the value is at least 1
    }
    
    public static int UniformDistribution(int minValue, int maxValue)
    {
        double randomValue = Random.Range(minValue, maxValue + 1); // Uniform distribution between minValue and maxValue
        return Math.Max(1, (int)Math.Round(randomValue)); // Ensure the value is at least 1
    }
}