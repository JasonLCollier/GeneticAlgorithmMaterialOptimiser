using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace WRCI411_Ass3_Collier
{
    class Program
    {

        static Random rnd = new Random();
        static void Main(string[] args)
        {
            int t = 0; //generation counter initialised to 0
            int ns = 1000; //population of ns chromosones
            int nts = 100; //number of individuals for tournament selection
            int initRange = 60; //its allele is initialised with a value between 0 and (initRange-1) Kgs
            double pm = 0.15; //mutation probability aka mutation rate
            double pc = 0.85; //crossover probability aka crossover rate
            double[,] population = new double[ns, 4]; //ns individuals with 4 genes each with real value representation
            double[,] offspring = new double[population.GetLength(0) - 1, population.GetLength(1)]; //ns-1 offspring to replace population (elite indiv remains to new population)
            int bestIndiv;
            double curGenFitness;
            double newGenFitness;
            int it_wo_change = 0;
            ArrayList FitnessvsGenerations = new ArrayList(); //used to store Fitness vs Generations data

            population = Initialisation(population, initRange);
            newGenFitness = GenerationalFitness(population, t);
            FitnessvsGenerations.Add(newGenFitness);
            curGenFitness = newGenFitness;

            while (t < 10000 && it_wo_change <= 100)
            {
                offspring = Reproduction(population, nts, pm, pc);
                population = SurvivorSelection(population, offspring);
                newGenFitness = GenerationalFitness(population, t + 1);
                FitnessvsGenerations.Add(newGenFitness);
                if (Math.Round(newGenFitness) == Math.Round(curGenFitness))
                    it_wo_change++;
                curGenFitness = newGenFitness;
                t++;
            }

            bestIndiv = Elitism(population);
            Console.WriteLine("Adamantium\tUnobtainium\tDilithium\tPandemonium");
            for (int j = 0; j < population.GetLength(1); j++)
            {
                Console.Write("{0}\t\t", Math.Round(population[bestIndiv, j], 1).ToString());
            }
            double[] test = { 11, 13.1, 18.3, 26.85 }; // {12, 0, 44, 7} R195693
            Console.WriteLine("\nTest fitness = {0}", Fitness(test));

            StreamWriter SW = new StreamWriter("FitnessvsGenerations.csv");
            for (int k = 0; k < FitnessvsGenerations.Count; k++)
            {
                SW.WriteLine((k + 1).ToString() + "," + FitnessvsGenerations[k].ToString());
            }
            SW.Close();

            Console.ReadLine();
        }

        public static double[,] Initialisation(double [,] population, int range)
        {
            for (int i = 0; i < population.GetLength(0); i++)
            {
                for (int j = 0; j < population.GetLength(1); j++)
                {
                    population[i, j] = rnd.Next(0, range);
                }
            }
            return population;
        }

        public static double Fitness(double[] individual)
        {
            double fitness;
            double profit;
            double cost;
            double income = 0;
            double platinum = 0;
            double iron = 0;
            double copper = 0;
            double kWh = 0;
            double discount = 0;
            //Calculate income, kwh, amount of platinum, iron, copper for individual
            for (int j = 0; j < individual.Length; j++)
            {
                switch (j)
                {
                    case 0:
                        income += individual[j] * 3000;
                        platinum += individual[j] * 0.2;
                        iron += individual[j] * 0.7;
                        copper += individual[j] * 0.1;
                        kWh += individual[j] * 25;
                        break;
                    case 1:
                        income += individual[j] * 3100;
                        platinum += individual[j] * 0.3;
                        iron += individual[j] * 0.2;
                        copper += individual[j] * 0.5;
                        kWh += individual[j] * 23;
                        break;
                    case 2:
                        income += individual[j] * 5200;
                        platinum += individual[j] * 0.8;
                        iron += individual[j] * 0.1;
                        copper += individual[j] * 0.1;
                        kWh += individual[j] * 35;
                        break;
                    case 3:
                        income += individual[j] * 2500;
                        platinum += individual[j] * 0.1;
                        iron += individual[j] * 0.5;
                        copper += individual[j] * 0.4;
                        kWh += individual[j] * 20;
                        break;
                }
            }
            //discount on copper
            if (copper > 8)
                discount = 0.1;
            //free iron
            iron -= platinum;
            if (iron < 0)
                iron = 0;
            //Calculate cost
            cost = (platinum * (1200 + 10 * platinum)) + (300 * iron) + (800 * copper * (1 - discount)) + (Math.Exp(0.005 * kWh));
            //fitness calculation
            profit = income - cost;
            fitness = profit;
            return fitness;
        }

        public static int TournamentSelection(double[,] population, int nts)
        {
            double[] individual = new double [population.GetLength(1)];
            double fitness;
            double bestFit = double.MinValue;
            int bestFitIndex = -1;
            int i;
            int count = 0;
            while (count < nts)
            {
                i = rnd.Next(0, population.GetLength(0));
                for (int j = 0; j < population.GetLength(1); j++)
                {
                    individual[j] = population[i, j];
                }
                fitness = Fitness(individual);
                if (fitness >= bestFit)
                {
                    bestFit = fitness;
                    bestFitIndex = i;
                }
                count++;
            }
            return bestFitIndex;
        }

        public static double[] UniformCrossover(double[,] population, int parent1, int parent2)
        {
            double[] offspring = new double[population.GetLength(1)];
            double px = 0.5;
            for (int j = 0; j < population.GetLength(1); j++)
            {
                if (rnd.NextDouble() <= px)
                {
                    offspring[j] = population[parent1, j];
                }
                else
                {
                    offspring[j] = population[parent2, j];
                }
            }
            return offspring;
        }

        public static double[] UniformMutation(double[] offspring, double[,] population, double pm)
        {
            int decision;
            double xmaxj;
            double xminj;
            for (int j = 0; j < population.GetLength(1); j++)
            {
                xmaxj = double.MinValue;
                xminj = double.MaxValue;
                decision = rnd.Next(0, 2);
                if (rnd.NextDouble() <= pm)
                {
                    if (decision == 0)
                    {
                        for (int i = 0; i < population.GetLength(0); i++)
                        {
                            if (population[i, j] > xmaxj)
                                xmaxj = population[i, j];
                        }
                        offspring[j] += Scale(rnd.NextDouble(), (xmaxj - offspring[j]), 0, 1, 0);
                    }
                    else if (decision == 1)
                    {
                        for (int i = 0; i < population.GetLength(0); i++)
                        {
                            if (population[i, j] < xminj)
                                xminj = population[i, j];
                        }
                        offspring[j] += Scale(rnd.NextDouble(), (offspring[j] - xminj), 0, 1, 0);
                    }
                }
            }
            return offspring;
        }

        public static double[,] Reproduction(double[,] population, int nts, double pm, double pc)
        {
            double[,] offspring = new double[population.GetLength(0) - 1, population.GetLength(1)];
            double[] individual = new double[population.GetLength(0)];
            int parent1, parent2;
            int count = 0;
            while (count < population.GetLength(0) - 1)
            {
                parent1 = TournamentSelection(population, nts);
                parent2 = TournamentSelection(population, nts);
                if (rnd.NextDouble() < pc)
                {
                    individual = UniformCrossover(population, parent1, parent2);
                    individual = UniformMutation(individual, population, pm);
                    for (int j = 0; j < population.GetLength(1); j++)
                    {
                        offspring[count, j] = individual[j];
                    }
                    count++;
                }
            }
            return offspring;
        }

        public static double[,] SurvivorSelection(double[,] population, double[,] offspring)
        {
            for (int j = 0; j < offspring.GetLength(1); j++)
            {
                population[population.GetLength(0) - 1, j] = population[Elitism(population), j];
            }

            for (int i = 0; i < offspring.GetLength(0); i++)
            {
                for (int j = 0; j < offspring.GetLength(1); j++)
                {
                    population[i, j] = offspring[i, j];
                }
            }

            return population;
        }

        public static int Elitism(double[,] population)
        {
            double[] individual = new double[population.GetLength(1)];
            double fitness;
            double bestFit = double.MinValue;
            int bestFitIndex = -1;
            for (int i = 0; i < population.GetLength(0); i++)
            {
                for (int j = 0; j < population.GetLength(1); j++)
                {
                    individual[j] = population[i, j];
                }
                fitness = Fitness(individual);
                if (fitness > bestFit)
                {
                    bestFit = fitness;
                    bestFitIndex = i;
                }
            }
            return bestFitIndex;
        }

        public static double Scale(double tu, double tsmax, double tsmin, double tumax, double tumin)
        {
            return ((tu - tumin) / (tumax - tumin)) * (tsmax - tsmin) + (tsmin);
        }

        public static double GenerationalFitness(double[,] population, int gen)
        {
            double[] curIndiv = new double[population.GetLength(1)];
            double aveFitness;
            double totalFitness = 0;
            for (int i = 0; i < population.GetLength(0); i++)
            {
                for (int j = 0; j < population.GetLength(1); j++)
                {
                    curIndiv[j] = population[i, j];
                }
                totalFitness += Fitness(curIndiv);
            }
            aveFitness = totalFitness / population.GetLength(0);
            Console.WriteLine("{0}\t\t{1}", gen, Math.Round(aveFitness, 2).ToString());
            return aveFitness;
        }
    }
}
