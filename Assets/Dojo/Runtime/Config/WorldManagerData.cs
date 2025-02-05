using System.Collections;
using System.Collections.Generic;
using Dojo.Starknet;
using Dojo.Torii;
using UnityEngine;

namespace Dojo
{
    [CreateAssetMenu(fileName = "WorldManagerData", menuName = "ScriptableObjects/WorldManagerData", order = 2)]
    public class WorldManagerData : ScriptableObject
    {
        [Header("RPC")]
        public string toriiUrl = "http://localhost:8080";
        public string rpcUrl = "http://localhost:5050";
        public string relayUrl = "/ip4/127.0.0.1/tcp/9090";
        public string relayWebrtcUrl;
        [Header("World")]
        public FieldElement worldAddress;
        public Query query = new Query(new CompositeClause(dojo_bindings.dojo.LogicalOperator.Or, new Clause[] {
            new MemberClause("dojo_starter-Position", "vec.x", dojo_bindings.dojo.ComparisonOperator.In, new MemberValue(new MemberValue[] { new Primitive {
                U32 = 10
            }, new Primitive {
                U32 = 13
            } }))
        }));
    }
}
