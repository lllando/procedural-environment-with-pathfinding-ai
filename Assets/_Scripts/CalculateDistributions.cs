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
}