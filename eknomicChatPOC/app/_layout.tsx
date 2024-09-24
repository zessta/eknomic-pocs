import { Stack } from "expo-router";

// export default function RootLayout() {
//   return (
//     <Stack>
//       <Stack.Screen name="index" />
//       <Stack.Screen name="index1" />
//     </Stack>
//   );
// }

import { AuthProvider } from "../context/AuthProvider";

export default function RootLayout() {
  return (
    <Stack
      // screenOptions={{
      //   ...
      // }}
    >
      <Stack.Screen
        name="(tabs)"
        options={{
          headerShown: false,
        }}
      />
      <Stack.Screen name="index" />
  {/*<Stack.Screen name="index1" /> */}
    </Stack>
  );
}
