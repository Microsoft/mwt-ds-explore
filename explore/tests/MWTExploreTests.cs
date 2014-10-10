﻿using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiWorldTesting;

namespace ExploreTests
{
    [TestClass]
    public class MWTExploreTests
    {
        /* 
        ** C# Tests do not need to be as extensive as those for C++. These tests should ensure
        ** the interactions between managed and native code are as expected.
        */
        [TestMethod]
        public void EpsilonGreedyStateful()
        {
            mwt.InitializeEpsilonGreedy<int>(Epsilon,
                new StatefulPolicyDelegate<int>(TestStatefulPolicyFunc),
                PolicyParams,
                NumActions);

            uint expectedAction = MWTExploreTests.TestStatefulPolicyFunc(
                PolicyParams,
                context);

            uint chosenAction = mwt.ChooseAction(context, UniqueKey);
            Assert.AreEqual(expectedAction, chosenAction);

            Tuple<uint, ulong> chosenActionAndKey = mwt.ChooseActionAndKey(context);
            Assert.AreEqual(expectedAction, chosenActionAndKey.Item1);

            INTERACTION[] interactions = mwt.GetAllInteractions();
            Assert.AreEqual(2, interactions.Length);

            Assert.AreEqual(2, interactions[0].ApplicationContext.Features.Length);
            Assert.AreEqual(0.9f, interactions[0].ApplicationContext.Features[1].X);
        }

        [TestMethod]
        public void EpsilonGreedyStateless()
        {
            mwt.InitializeEpsilonGreedy(Epsilon,
                new StatelessPolicyDelegate(TestStatelessPolicyFunc),
                NumActions);

            uint expectedAction = MWTExploreTests.TestStatelessPolicyFunc(context);

            uint chosenAction = mwt.ChooseAction(context, UniqueKey);
            Assert.AreEqual(expectedAction, chosenAction);

            Tuple<uint, ulong> chosenActionAndKey = mwt.ChooseActionAndKey(context);
            Assert.AreEqual(expectedAction, chosenActionAndKey.Item1);

            INTERACTION[] interactions = mwt.GetAllInteractions();
            Assert.AreEqual(2, interactions.Length);

            Assert.AreEqual(2, interactions[0].ApplicationContext.Features.Length);
            Assert.AreEqual(0.9f, interactions[0].ApplicationContext.Features[1].X);
        }

        [TestMethod]
        public void TauFirstStateful()
        {
            mwt.InitializeTauFirst<int>(Tau,
                new StatefulPolicyDelegate<int>(TestStatefulPolicyFunc),
                PolicyParams,
                NumActions);

            uint expectedAction = MWTExploreTests.TestStatefulPolicyFunc(
                PolicyParams,
                context);

            uint chosenAction = mwt.ChooseAction(context, UniqueKey);
            Assert.AreEqual(expectedAction, chosenAction);

            Tuple<uint, ulong> chosenActionAndKey = mwt.ChooseActionAndKey(context);
            Assert.AreEqual(expectedAction, chosenActionAndKey.Item1);

            INTERACTION[] interactions = mwt.GetAllInteractions();
            Assert.AreEqual(0, interactions.Length);
        }

        [TestMethod]
        public void TauFirstStateless()
        {
            mwt.InitializeTauFirst(Tau,
                new StatelessPolicyDelegate(TestStatelessPolicyFunc),
                NumActions);

            uint expectedAction = MWTExploreTests.TestStatelessPolicyFunc(context);

            uint chosenAction = mwt.ChooseAction(context, UniqueKey);
            Assert.AreEqual(expectedAction, chosenAction);

            Tuple<uint, ulong> chosenActionAndKey = mwt.ChooseActionAndKey(context);
            Assert.AreEqual(expectedAction, chosenActionAndKey.Item1);

            INTERACTION[] interactions = mwt.GetAllInteractions();
            Assert.AreEqual(0, interactions.Length);
        }

        [TestMethod]
        public void BaggingStateful()
        {
            StatefulPolicyDelegate<int>[] funcs = new StatefulPolicyDelegate<int>[Bags];
            int[] funcParams = new int[Bags];
            for (int i = 0; i < Bags; i++)
			{
                funcs[i] = new StatefulPolicyDelegate<int>(TestStatefulPolicyFunc);
                funcParams[i] = PolicyParams;
			}

            mwt.InitializeBagging(Bags, funcs, funcParams, NumActions);

            uint expectedAction = MWTExploreTests.TestStatefulPolicyFunc(
                PolicyParams,
                context);

            uint chosenAction = mwt.ChooseAction(context, UniqueKey);
            Assert.AreEqual(expectedAction, chosenAction);

            Tuple<uint, ulong> chosenActionAndKey = mwt.ChooseActionAndKey(context);
            Assert.AreEqual(expectedAction, chosenActionAndKey.Item1);

            INTERACTION[] interactions = mwt.GetAllInteractions();
            Assert.AreEqual(2, interactions.Length);

            Assert.AreEqual(2, interactions[0].ApplicationContext.Features.Length);
            Assert.AreEqual(0.9f, interactions[0].ApplicationContext.Features[1].X);
        }

        [TestMethod]
        public void BaggingStateless()
        {
            StatelessPolicyDelegate[] funcs = new StatelessPolicyDelegate[Bags];
            for (int i = 0; i < Bags; i++)
            {
                funcs[i] = new StatelessPolicyDelegate(TestStatelessPolicyFunc);
            }

            mwt.InitializeBagging(Bags, funcs, NumActions);

            uint expectedAction = MWTExploreTests.TestStatelessPolicyFunc(context);

            uint chosenAction = mwt.ChooseAction(context, UniqueKey);
            Assert.AreEqual(expectedAction, chosenAction);

            Tuple<uint, ulong> chosenActionAndKey = mwt.ChooseActionAndKey(context);
            Assert.AreEqual(expectedAction, chosenActionAndKey.Item1);

            INTERACTION[] interactions = mwt.GetAllInteractions();
            Assert.AreEqual(2, interactions.Length);

            Assert.AreEqual(2, interactions[0].ApplicationContext.Features.Length);
            Assert.AreEqual(0.9f, interactions[0].ApplicationContext.Features[1].X);
        }

        [TestMethod]
        public void SoftmaxStateful()
        {
            mwt.InitializeSoftmax<int>(Lambda,
                new StatefulScorerDelegate<int>(TestStatefulScorerFunc), 
                PolicyParams, NumActions);

            uint numDecisions = (uint)(NumActions * Math.Log(NumActions * 1.0) + Math.Log(NumActionsCover * 1.0 / NumActions) * C * NumActions);
            uint[] actions = new uint[NumActions];
            
            for (uint i = 0; i < numDecisions; i++)
            {
                Tuple<uint, ulong> actionAndKey = mwt.ChooseActionAndKey(context);
                actions[ActionID.Make_ZeroBased(actionAndKey.Item1)]++;
            }
            
            for (uint i = 0; i < NumActions; i++)
            {
                Assert.IsTrue(actions[i] > 0);
            }

            mwt.GetAllInteractions();
        }

        [TestMethod]
        public void SoftmaxStateless()
        {
            mwt.InitializeSoftmax(Lambda,
                new StatelessScorerDelegate(TestStatelessScorerFunc),
                NumActions);

            uint numDecisions = (uint)(NumActions * Math.Log(NumActions * 1.0) + Math.Log(NumActionsCover * 1.0 / NumActions) * C * NumActions);
            uint[] actions = new uint[NumActions];

            for (uint i = 0; i < numDecisions; i++)
            {
                Tuple<uint, ulong> actionAndKey = mwt.ChooseActionAndKey(context);
                actions[ActionID.Make_ZeroBased(actionAndKey.Item1)]++;
            }

            for (uint i = 0; i < NumActions; i++)
            {
                Assert.IsTrue(actions[i] > 0);
            }

            mwt.GetAllInteractions();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mwt = new MwtExplorer();

            features = new FEATURE[2];
            features[0].X = 0.5f;
            features[0].WeightIndex = 1;
            features[1].X = 0.9f;
            features[1].WeightIndex = 2;

            context = new CONTEXT(features, "Other C# test context");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            mwt.Unintialize();
        }

        private static UInt32 TestStatefulPolicyFunc(int policyParams, CONTEXT context)
        {
            return ActionID.Make_OneBased((uint)(policyParams + context.Features.Length) % MWTExploreTests.NumActions);
        }

        private static UInt32 TestStatelessPolicyFunc(CONTEXT context)
        {
            return ActionID.Make_OneBased((uint)context.Features.Length % MWTExploreTests.NumActions);
        }

        private static void TestStatefulScorerFunc(int policyParams, CONTEXT applicationContext, float[] scores)
        {
            for (uint i = 0; i < scores.Length; i++)
            {
                scores[i] = (int)policyParams + i;
            }
        }

        private static void TestStatelessScorerFunc(CONTEXT applicationContext, float[] scores)
        {
            for (uint i = 0; i < scores.Length; i++)
            {
                scores[i] = applicationContext.Features.Length + i;
            }
        }

        private static readonly uint NumActionsCover = 100;
        private static readonly float C = 5;

        private static readonly uint NumActions = 10;
        private static readonly float Epsilon = 0;
        private static readonly uint Tau = 0;
        private static readonly uint Bags = 2;
        private static readonly float Lambda = 0.5f;
        private static readonly int PolicyParams = 1003;
        private static readonly string UniqueKey = "ManagedTestId";

        private MwtExplorer mwt;
        private FEATURE[] features;
        private CONTEXT context;
    }
}