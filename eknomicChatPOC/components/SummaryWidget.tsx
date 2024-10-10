import React from 'react';
import { View, Text, Button, StyleSheet } from 'react-native';
import { IMessage } from 'react-native-gifted-chat';

interface SummaryWidgetProps {
  messages: IMessage[];
  onYes: () => void;
  onNo: () => void;
}

const SummaryWidget: React.FC<SummaryWidgetProps> = ({ messages, onYes, onNo }) => {
  const getSummary = () => {
    const lastMessages = messages.slice(-10);
    // return lastMessages.map(msg => msg.text).join(' | ');
    return 'Do you want summary of last 10 messages?'
  };

  return (
    <View style={styles.container}>
      <Text style={styles.summaryText}>{getSummary()}</Text>
      <View style={styles.buttonContainer}>
        <Button title="Yes" onPress={onYes} />
        <Button title="No" onPress={onNo} />
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    padding: 10,
    backgroundColor: '#f8f8f8',
    borderRadius: 10,
    marginVertical: 10,
  },
  summaryText: {
    marginBottom: 10,
    fontSize: 16,
  },
  buttonContainer: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    gap: 10
  },
});

export default SummaryWidget;
