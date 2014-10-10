﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiWorldTesting;

namespace cs_test
{
    class ExploreSample
    {
        private static UInt32 SampleStatefulPolicyFunc(int parameters, CONTEXT context)
        {
            return (uint)((parameters + context.Features.Length) % 10 + 1);
        }

        private static UInt32 SampleStatefulPolicyFunc2(int parameters, CONTEXT context)
        {
            return (uint)((parameters + context.Features.Length) % 10 + 2);
        }

        private static UInt32 SampleStatefulPolicyFunc(CustomParams parameters, CONTEXT context)
        {
            return (uint)((parameters.Value1 + parameters.Value2 + context.Features.Length) % 10 + 1);
        }

        private static UInt32 SampleStatelessPolicyFunc(CONTEXT applicationContext)
        {
            return (UInt32)applicationContext.Features.Length;
        }

        private static UInt32 SampleStatelessPolicyFunc2(CONTEXT applicationContext)
        {
            return (UInt32)applicationContext.Features.Length + 1;
        }

        private static void SampleStatefulScorerFunc(int parameters, CONTEXT applicationContext, float[] scores)
        {
            for (uint i = 0; i < scores.Length; i++)
            {
                scores[i] = (int)parameters + i;
            }
        }

        private static void SampleStatelessScorerFunc(CONTEXT applicationContext, float[] scores)
        {
            for (uint i = 0; i < scores.Length; i++)
            {
                scores[i] = applicationContext.Features.Length + i;
            }
        }

        class CustomParams
        {
            public int Value1;
            public int Value2;
        }

        public static void Run()
        {
            MwtExplorer mwt = new MwtExplorer();

            uint numActions = 10;
            
            float epsilon = 0.2f;
            uint tau = 0;
            uint bags = 2;
            float lambda = 0.5f;

            int policyParams = 1003;
            CustomParams customParams = new CustomParams() { Value1 = policyParams, Value2 = policyParams + 1 };

            /*** Initialize Epsilon-Greedy explore algorithm using a default policy function that accepts parameters ***/
            //mwt.InitializeEpsilonGreedy<int>(epsilon, new StatefulPolicyDelegate<int>(SampleStatefulPolicyFunc), policyParams, numActions);

            /*** Initialize Epsilon-Greedy explore algorithm using a stateless default policy function ***/
            //mwt.InitializeEpsilonGreedy(epsilon, new StatelessPolicyDelegate(SampleStatelessPolicyFunc), numActions);

            /*** Initialize Tau-First explore algorithm using a default policy function that accepts parameters ***/
            //mwt.InitializeTauFirst<CustomParams>(tau, new StatefulPolicyDelegate<CustomParams>(SampleStatefulPolicyFunc), customParams, numActions);

            /*** Initialize Tau-First explore algorithm using a stateless default policy function ***/
            //mwt.InitializeTauFirst(tau, new StatelessPolicyDelegate(SampleStatelessPolicyFunc), numActions);

            /*** Initialize Bagging explore algorithm using a default policy function that accepts parameters ***/
            //StatefulPolicyDelegate<int>[] funcs = 
            //{
            //    new StatefulPolicyDelegate<int>(SampleStatefulPolicyFunc), 
            //    new StatefulPolicyDelegate<int>(SampleStatefulPolicyFunc2) 
            //};
            //int[] parameters = { policyParams, policyParams };
            //mwt.InitializeBagging<int>(bags, funcs, parameters, numActions);

            /*** Initialize Bagging explore algorithm using a stateless default policy function ***/
            //StatelessPolicyDelegate[] funcs = 
            //{
            //    new StatelessPolicyDelegate(SampleStatelessPolicyFunc), 
            //    new StatelessPolicyDelegate(SampleStatelessPolicyFunc2) 
            //};
            //mwt.InitializeBagging(bags, funcs, numActions);

            /*** Initialize Softmax explore algorithm using a default policy function that accepts parameters ***/
            //mwt.InitializeSoftmax<int>(lambda, new StatefulScorerDelegate<int>(SampleStatefulScorerFunc), policyParams, numActions);

            /*** Initialize Softmax explore algorithm using a stateless default policy function ***/
            mwt.InitializeSoftmax(lambda, new StatelessScorerDelegate(SampleStatelessScorerFunc), numActions);

            FEATURE[] f = new FEATURE[2];
            f[0].X = 0.5f;
            f[0].WeightIndex = 1;
            f[1].X = 0.9f;
            f[1].WeightIndex = 2;

            string otherContext = "Some other context data that might be helpful to log";
            CONTEXT context = new CONTEXT(f, otherContext);

            UInt32 chosenAction = mwt.ChooseAction(context, "myId");

            string interactions = mwt.GetAllInteractionsAsString();

            mwt.Unintialize();

            Console.WriteLine(chosenAction);
            Console.WriteLine(interactions);
        }

        public static void Clock()
        {
            float epsilon = .2f;
            int policyParams = 1003;
            string uniqueKey = "clock";
            int numFeatures = 1000;
            int numIter = 1000;
            int numWarmup = 100;
            int numInteractions = 1;
            uint numActions = 10;
            string otherContext = null;
            
            double timeInit = 0, timeChoose = 0, timeSerializedLog = 0, timeTypedLog = 0;

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            for (int iter = 0; iter < numIter + numWarmup; iter++)
            {
                watch.Restart();
                
                MwtExplorer mwt = new MwtExplorer();
                mwt.InitializeEpsilonGreedy<int>(epsilon, new StatefulPolicyDelegate<int>(SampleStatefulPolicyFunc), policyParams, numActions);

                timeInit += (iter < numWarmup) ? 0 : watch.Elapsed.TotalMilliseconds;

                FEATURE[] f = new FEATURE[numFeatures];
                for (int i = 0; i < numFeatures; i++)
                {
                    f[i].WeightIndex = (uint)i + 1;
                    f[i].X = 0.5f;
                }
                
                watch.Restart();

                CONTEXT context = new CONTEXT(f, otherContext);

                for (int i = 0; i < numInteractions / 2; i++)
                {
                    mwt.ChooseAction(context, uniqueKey);
                    mwt.ChooseActionAndKey(context);
                }

                timeChoose += (iter < numWarmup) ? 0 : watch.Elapsed.TotalMilliseconds;

                watch.Restart();

                string interactions = mwt.GetAllInteractionsAsString();

                timeSerializedLog += (iter < numWarmup) ? 0 : watch.Elapsed.TotalMilliseconds;

                for (int i = 0; i < numInteractions / 2; i++)
                {
                    mwt.ChooseAction(context, uniqueKey);
                    mwt.ChooseActionAndKey(context);
                }

                watch.Restart();

                mwt.GetAllInteractions();

                timeTypedLog += (iter < numWarmup) ? 0 : watch.Elapsed.TotalMilliseconds;
            }
            Console.WriteLine("--- PER ITERATION ---");
            Console.WriteLine("# iterations: {0}, # interactions: {1}, # context features {2}", numIter, numInteractions, numFeatures);
            Console.WriteLine("Init: {0} micro", timeInit * 1000 / numIter);
            Console.WriteLine("Choose Action: {0} micro", timeChoose * 1000 / (numIter * numInteractions));
            Console.WriteLine("Get Serialized Log: {0} micro", timeSerializedLog * 1000 / numIter);
            Console.WriteLine("Get Typed Log: {0} micro", timeTypedLog * 1000 / numIter);
            Console.WriteLine("--- TOTAL TIME: {0} micro", (timeInit + timeChoose + timeSerializedLog + timeTypedLog) * 1000);
        }
    }
}