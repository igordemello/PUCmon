using System;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{
	public class MonoPInvokeCallbackAttribute : System.Attribute
	{
		public Type type;
		public MonoPInvokeCallbackAttribute( Type t ) { type = t; }
	}
}