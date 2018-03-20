﻿using LDPDatapoints.Subscriptions;
using System;
using System.Linq;
using System.Net;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;

namespace LDPDatapoints
{
    public class Resource<T>
    {

        protected string route { get; }
        protected Graph RDFGraph { get; }
        protected CompressingTurtleWriter TtlWriter { get; }
        protected HttpRequestListener RequestListener { get; }
        protected ISubscription[] Subscriptions { get; }

        protected T _value;
        public virtual T Value
        {
            get { return _value; }
            set { _value = value; /* .. trigger update event */ }
        }

        public Resource(T value, string route)
        {
            this.route = route;
            TtlWriter = new CompressingTurtleWriter();
            RequestListener = new HttpRequestListener(route);
            RequestListener.OnGet += onGet;
            RDFGraph = new Graph();
            RDFGraph.NamespaceMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
            RDFGraph = buildGraph(value);
        }

        protected virtual void onGet(object sender, HttpEventArgs e)
        {
            HttpListenerRequest request = e.request;
            HttpListenerResponse response = e.response;

            // use JSON-LD representation only if explicitly requested
            if (request.AcceptTypes.Contains("application/ld+json") && !(request.AcceptTypes.Contains("text/turtle")))
            {
                // TODO: json-ld representation
                response.OutputStream.Write(Encoding.UTF8.GetBytes("NOT YET IMPLEMENTED"), 0, "NOT YET IMPLEMENTED".Length);
            }
            else
            {
                System.IO.StringWriter sw = new System.IO.StringWriter();
                TtlWriter.Save(RDFGraph, sw);
                string graph = sw.ToString();
                response.OutputStream.Write(Encoding.UTF8.GetBytes(graph), 0, graph.Length);
            }

            response.Close();
        }

        protected virtual void NotifySubscriptions(object sender, EventArgs e)
        {
            throw new NotImplementedException("Instance of Resource<T> does not implement notification of subscriptions");
        }

        protected virtual Graph buildGraph(T value)
        {
            //XmlSerializer serializer = new XmlSerializer(typeof(T));
            //var sw = new System.IO.StringWriter();
            //serializer.Serialize(sw, value);
            //var xmlvalue = sw.ToString();
            var graph = new Graph();
            var o = RDFGraph.CreateLiteralNode(value.ToString(), new Uri("http://localhost:3333/todo"));
            var p = RDFGraph.CreateUriNode("owl:hasValue");
            var s = RDFGraph.CreateUriNode(new Uri(route));
            graph.Assert(new Triple(s, p, o));
            return graph;
        }
    }
}
